using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Services;
using Microsoft.EntityFrameworkCore;
using ApparelPro.Data;

namespace ApparelPro.AI.BackgroundJobs;

/// <summary>
/// Background service that periodically syncs ERP entity data into the vector store.
///
/// 🎓 WHAT IS A BACKGROUND SERVICE?
/// In .NET, a BackgroundService is code that runs continuously alongside your API.
/// It starts when the application starts and stops when the application shuts down.
/// It runs on its own thread, so it doesn't block API requests.
///
/// Think of it like a cron job, but built into your application:
///   "Every 15 minutes, check for updated entities and re-embed them."
///
/// 🎓 WHY DO WE NEED THIS?
/// Without sync, the vector store would only know about entities that existed
/// when you first loaded the data. When someone adds a new Style or updates
/// a PO in ApparelPro, the vector store needs to learn about it too.
///
/// The sync job handles this automatically:
///   1. Query SQL Server for entities modified since the last sync
///   2. Convert them to text chunks (EntityChunkerService)
///   3. Generate embeddings (EmbeddingService)
///   4. Store vectors in Qdrant (QdrantVectorStoreService)
///
/// 🎓 ARCHITECTURE DECISIONS:
///
/// Q: Why not sync in real-time (on every save)?
/// A: We COULD trigger embedding on every entity save (via events or middleware),
///    but batch sync is simpler and more resilient:
///    - If OpenAI's API is temporarily down, the sync retries next cycle
///    - Bulk operations (importing 500 styles) don't cause 500 API calls
///    - The sync interval (15 min) is fast enough for ERP use cases
///    - You can always add real-time sync later as an enhancement
///
/// Q: Why BackgroundService instead of Hangfire/Quartz?
/// A: For a simple periodic timer, BackgroundService is built into .NET —
///    no extra NuGet packages or database tables needed. If you later need
///    more complex scheduling (retries, job queues, dashboard), consider
///    Hangfire. But for now, KISS (Keep It Simple, Stupid).
///
/// 🎓 SCOPED SERVICES IN BACKGROUND JOBS:
/// BackgroundService is a SINGLETON (lives for the app's entire lifetime).
/// But DbContext (ApparelProDbContext) is SCOPED (one per HTTP request).
/// You can't inject a scoped service into a singleton!
///
/// The solution: IServiceScopeFactory. On each tick, we create a new scope,
/// resolve the scoped services within it, do our work, and dispose the scope.
/// This is the standard .NET pattern for background services that need DbContext.
/// </summary>
public sealed class EmbeddingSyncJob : BackgroundService
{
    // ── Dependencies ─────────────────────────────────────

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RagSettings _ragSettings;
    private readonly ILogger<EmbeddingSyncJob> _logger;

    /// <summary>
    /// 🎓 CONSTRUCTOR INJECTION:
    /// Notice we inject IServiceScopeFactory, NOT the actual services.
    /// This is because:
    ///   - EmbeddingSyncJob is a SINGLETON (background services always are)
    ///   - The services it needs (DbContext, etc.) are SCOPED
    ///   - We use the factory to create a scope on each timer tick
    ///
    /// We DO inject IOptions directly because configuration is singleton-safe.
    /// </summary>
    public EmbeddingSyncJob(
        IServiceScopeFactory scopeFactory,
        IOptions<RagSettings> ragSettings,
        ILogger<EmbeddingSyncJob> logger)
    {
        _scopeFactory = scopeFactory;
        _ragSettings = ragSettings.Value;
        _logger = logger;
    }

    // ── Main execution loop ──────────────────────────────

    /// <summary>
    /// 🎓 THE HEART OF THE BACKGROUND SERVICE:
    /// ExecuteAsync is called once when the application starts.
    /// It runs until the stoppingToken is cancelled (app shutdown).
    ///
    /// Our implementation:
    ///   1. Wait 30 seconds on startup (let the app fully initialise)
    ///   2. Ensure the Qdrant collection exists
    ///   3. Run the sync loop:
    ///      a. Sync all entity types
    ///      b. Wait for SyncIntervalMinutes
    ///      c. Repeat
    ///
    /// 🎓 ERROR HANDLING:
    /// The entire sync cycle is wrapped in try/catch. If Qdrant is down,
    /// the OpenAI API fails, or the database is unreachable, we:
    ///   1. Log the error
    ///   2. Wait for the next cycle
    ///   3. Try again
    ///
    /// We NEVER let an exception kill the background service.
    /// A crashed sync job means the vector store silently goes stale —
    /// the API still works, but RAG answers become increasingly outdated.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "EmbeddingSyncJob starting. Sync interval: {Interval} minutes",
            _ragSettings.SyncIntervalMinutes);

        // ── Startup delay ────────────────────────────
        // 🎓 WHY WAIT?
        // On application startup, the database might still be migrating,
        // Qdrant might not be fully ready, and other services might be initialising.
        // A 30-second delay gives everything time to settle.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return; // App is shutting down during startup delay
        }

        // ── Ensure collection exists ─────────────────
        // 🎓 COLLECTION CREATION:
        // Create the Qdrant collection if it doesn't exist.
        // This is idempotent — safe to call every startup.
        try
        {
            using var initScope = _scopeFactory.CreateScope();
            var vectorStore = initScope.ServiceProvider
                .GetRequiredService<IVectorStoreService>();
            await vectorStore.EnsureCollectionExistsAsync(stoppingToken);

            _logger.LogInformation("Qdrant collection verified. Starting sync loop.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to initialise Qdrant collection. " +
                "Sync will retry on next cycle. Is Qdrant running at {Url}?",
                _ragSettings.QdrantUrl);
        }

        // ── Sync loop ────────────────────────────────
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunSyncCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // Clean shutdown
            }
            catch (Exception ex)
            {
                // 🎓 RESILIENCE: Log and continue.
                // The next cycle might succeed if the issue was transient
                // (network blip, API rate limit, temporary DB lock).
                _logger.LogError(ex,
                    "EmbeddingSyncJob cycle failed. Will retry in {Interval} minutes.",
                    _ragSettings.SyncIntervalMinutes);
            }

            // ── Wait for next cycle ──────────────────
            try
            {
                await Task.Delay(
                    TimeSpan.FromMinutes(_ragSettings.SyncIntervalMinutes),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // App is shutting down
            }
        }

        _logger.LogInformation("EmbeddingSyncJob stopping.");
    }

    // ── Sync cycle ───────────────────────────────────────

    /// <summary>
    /// 🎓 ONE SYNC CYCLE:
    /// This method runs every SyncIntervalMinutes and processes all entity types.
    ///
    /// Current implementation: FULL SYNC
    ///   - Queries ALL entities and re-embeds everything
    ///   - Simple but not efficient for large datasets
    ///
    /// Future improvement: INCREMENTAL SYNC
    ///   - Track a "last synced" timestamp
    ///   - Only query entities modified since then
    ///   - Much faster for ongoing operations
    ///
    /// We start with full sync because:
    ///   1. It's simpler to implement and debug
    ///   2. For ApparelPro's scale (~1000s of entities), it's fast enough
    ///   3. It guarantees no entity is missed
    ///   4. We can optimise later when we have real performance data
    ///
    /// 🎓 IMPORTANT: IServiceScope
    /// We create a fresh scope for each sync cycle. This gives us:
    ///   - A fresh DbContext (no stale cached entities)
    ///   - Proper disposal of all scoped services
    ///   - No memory leaks from long-lived DbContext tracking
    /// </summary>
    private async Task RunSyncCycleAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting embedding sync cycle...");

        using var scope = _scopeFactory.CreateScope();

        var embeddingService = scope.ServiceProvider
            .GetRequiredService<IEmbeddingService>();
        var vectorStore = scope.ServiceProvider
            .GetRequiredService<IVectorStoreService>();
        var chunker = scope.ServiceProvider
            .GetRequiredService<EntityChunkerService>();

        // ── Ensure collection exists (retry if failed at startup) ──
        await vectorStore.EnsureCollectionExistsAsync(stoppingToken);

        // ── Sync each entity type ────────────────────
        // 🎓 TODO: These methods will be implemented when we wire up
        // the data access layer. For now, they're placeholder methods
        // that log what they would do. This lets you:
        //   1. Run the app and verify the background service starts
        //   2. See the sync cycle in the logs
        //   3. Add real data access one entity type at a time

        // 🎓 PHASE C: Each fetch method now queries ApparelProDbContext for real data,
        // joins lookup tables (Buyers, GarmentTypes) for display names, and calls
        // the EntityChunkerService to produce embeddable text chunks.
        // We pass the chunker explicitly rather than resolving it inside each method.

        await SyncEntityTypeAsync<object>(
            entityType: "Style",
            fetchEntities: async () => await FetchStylesForSyncAsync(scope, chunker, stoppingToken),
            chunker: chunker,
            embeddingService: embeddingService,
            vectorStore: vectorStore,
            stoppingToken: stoppingToken);

        await SyncEntityTypeAsync<object>(
            entityType: "PurchaseOrder",
            fetchEntities: async () => await FetchPurchaseOrdersForSyncAsync(scope, chunker, stoppingToken),
            chunker: chunker,
            embeddingService: embeddingService,
            vectorStore: vectorStore,
            stoppingToken: stoppingToken);

        await SyncEntityTypeAsync<object>(
            entityType: "Buyer",
            fetchEntities: async () => await FetchBuyersForSyncAsync(scope, chunker, stoppingToken),
            chunker: chunker,
            embeddingService: embeddingService,
            vectorStore: vectorStore,
            stoppingToken: stoppingToken);

        await SyncEntityTypeAsync<object>(
            entityType: "Supplier",
            fetchEntities: async () => await FetchSuppliersForSyncAsync(scope, chunker, stoppingToken),
            chunker: chunker,
            embeddingService: embeddingService,
            vectorStore: vectorStore,
            stoppingToken: stoppingToken);

        // 🎓 PHASE 2 STEP 6 — SOP → RAG Embedding:
        // Sync Standard Operating Procedures into the vector store so RAG can
        // surface company rules when users ask about procedures, compliance,
        // packaging requirements, quality standards, etc. Each SOP's metadata
        // (category, applicability rules, date range) is included in the chunk
        // text to improve retrieval relevance.
        await SyncEntityTypeAsync<object>(
            entityType: "Sop",
            fetchEntities: async () => await FetchSopsForSyncAsync(scope, chunker, stoppingToken),
            chunker: chunker,
            embeddingService: embeddingService,
            vectorStore: vectorStore,
            stoppingToken: stoppingToken);

        // ── Report stats ─────────────────────────────
        var totalPoints = await vectorStore.GetPointCountAsync(stoppingToken);
        _logger.LogInformation(
            "Embedding sync cycle complete. Total vectors in store: {Count}",
            totalPoints);
    }

    // ── Generic sync method ──────────────────────────────

    /// <summary>
    /// 🎓 GENERIC SYNC PATTERN:
    /// This method handles the sync pipeline for any entity type:
    ///   1. Fetch entities → 2. Chunk → 3. Embed → 4. Store
    ///
    /// By making this generic, we avoid duplicating the embed-and-store
    /// logic for each entity type. Only the fetch and chunk steps differ.
    /// </summary>
    private async Task SyncEntityTypeAsync<T>(
        string entityType,
        Func<Task<List<EntityChunk>>> fetchEntities,
        EntityChunkerService chunker,
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStore,
        CancellationToken stoppingToken)
    {
        try
        {
            // Step 1: Fetch and chunk entities
            var chunks = await fetchEntities();
            if (chunks.Count == 0)
            {
                _logger.LogDebug("No {EntityType} entities to sync.", entityType);
                return;
            }

            _logger.LogInformation(
                "Syncing {Count} chunks for entity type '{EntityType}'",
                chunks.Count, entityType);

            // Step 2: Generate embeddings in batch
            // 🎓 BATCH SIZE:
            // We process chunks in batches of 100 to:
            //   - Stay within OpenAI's rate limits
            //   - Avoid massive single API calls that might timeout
            //   - Allow progress logging
            const int batchSize = 100;

            for (var i = 0; i < chunks.Count; i += batchSize)
            {
                stoppingToken.ThrowIfCancellationRequested();

                var batch = chunks.Skip(i).Take(batchSize).ToList();
                var texts = batch.Select(c => c.Text).ToList();

                // Step 3: Embed the batch
                var vectors = await embeddingService.EmbedBatchAsync(texts, stoppingToken);

                // Step 4: Build VectorPoints and upsert
                // 🎓 ZIP: Combine chunks and their vectors into VectorPoints.
                // chunk[0] gets vector[0], chunk[1] gets vector[1], etc.
                var points = batch.Zip(vectors, (chunk, vector) =>
                    new VectorPoint
                    {
                        Id = chunk.PointId,
                        Vector = vector,
                        Payload = new Dictionary<string, object>
                        {
                            ["entityType"] = chunk.EntityType,
                            ["entityKey"] = chunk.EntityKey,
                            ["chunkText"] = chunk.Text,
                            ["chunkIndex"] = chunk.ChunkIndex,
                            ["totalChunks"] = chunk.TotalChunks,
                            ["updatedAt"] = DateTime.UtcNow.ToString("O")
                        }
                    }).ToList();

                await vectorStore.UpsertAsync(points, stoppingToken);

                _logger.LogDebug(
                    "Synced batch {BatchStart}-{BatchEnd} of {Total} {EntityType} chunks",
                    i + 1, Math.Min(i + batchSize, chunks.Count),
                    chunks.Count, entityType);
            }

            _logger.LogInformation(
                "Completed syncing {Count} {EntityType} chunks.",
                chunks.Count, entityType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to sync entity type '{EntityType}'. " +
                "Other entity types will still be synced.",
                entityType);
            // Don't rethrow — let other entity types continue syncing
        }
    }

    // ── Entity fetch methods (wired to ApparelProDbContext) ──

    /// <summary>
    /// 🎓 PHASE C: REAL DATA FETCH — Styles
    ///
    /// This method is the heart of the RAG pipeline for Style entities.
    /// Here's what it does step by step:
    ///
    /// 1. RESOLVE DbContext from the scoped service provider
    ///    (Remember: Background services are singletons, DbContext is scoped —
    ///     that's why we use IServiceScopeFactory and resolve per-cycle)
    ///
    /// 2. LOAD LOOKUP TABLES into in-memory dictionaries:
    ///    - Buyers → Dictionary&lt;int, string&gt; (BuyerCode → Name)
    ///    - GarmentTypes → Dictionary&lt;int, string&gt; (Id → TypeName)
    ///    - Suppliers → Dictionary&lt;int, string&gt; (SupplierCode → Name)
    ///    These are small reference tables (~100s of rows), so loading them
    ///    into memory is fast and avoids N+1 queries later.
    ///
    /// 3. QUERY ALL STYLES with AsNoTracking()
    ///    (AsNoTracking = read-only, no change tracking overhead.
    ///     We're only reading — we never modify entities in the sync job.)
    ///
    /// 4. QUERY MATERIAL COST PROFILES and group them by the composite key
    ///    (BuyerCode, Order, TypeCode, StyleCode) that links them to each Style.
    ///    🎓 WHY NOT Include()? Because Style has NO navigation property to
    ///    StyleMaterialCostProfile. The EF model uses composite keys without
    ///    navigation properties. So we load materials separately and join in memory.
    ///
    /// 5. FOR EACH STYLE: Build the material dictionaries and call ChunkStyle()
    ///    to produce embeddable text chunks.
    ///
    /// 🎓 WHY IN-MEMORY JOINS INSTEAD OF LINQ JOINS?
    /// For the lookup tables (Buyers, GarmentTypes, Suppliers), we load them
    /// once and do Dictionary lookups. This is:
    ///   - Simpler than complex LINQ joins (easier to debug)
    ///   - Faster for repeated lookups (dictionary = O(1))
    ///   - More memory-efficient than materialising joined entities
    ///   - Avoids issues with [NotMapped] properties (Buyer, Type on Style)
    /// </summary>
    private async Task<List<EntityChunk>> FetchStylesForSyncAsync(
        IServiceScope scope,
        EntityChunkerService chunker,
        CancellationToken stoppingToken)
    {
        // ── Step 1: Resolve DbContext from the scoped provider ──
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApparelProDbContext>();

        // ── Step 2: Load lookup tables into dictionaries ────────
        // 🎓 These are small reference tables. Loading them all into memory
        // avoids repeated database round-trips when we process each style.

        var buyers = await dbContext.Buyers
            .AsNoTracking()
            .ToDictionaryAsync(b => b.BuyerCode, b => b.Name, stoppingToken);

        var garmentTypes = await dbContext.GarmentTypes
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.TypeName, stoppingToken);

        var suppliers = await dbContext.Suppliers
            .AsNoTracking()
            .ToDictionaryAsync(s => s.SupplierCode, s => s.Name, stoppingToken);

        // ── Step 3: Load all styles ─────────────────────────────
        // 🎓 AsNoTracking(): Since we're only reading (never saving changes),
        // we tell EF Core not to track these entities. This saves memory
        // and CPU — EF doesn't need to snapshot property values for change detection.
        var styles = await dbContext.Styles
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        _logger.LogInformation("Fetched {Count} styles from database", styles.Count);

        // ── Step 4: Load material cost profiles grouped by style ──
        // 🎓 WHY StyleMaterialCostProfiles AND NOT StyleMaterialConsumptionLedgers?
        // CostProfiles have the item Description (material name), UnitPrice,
        // and SupplierCode — the information most useful for RAG text chunks.
        // ConsumptionLedgers have per-garment quantities which are more granular
        // detail than needed for the initial RAG search context.
        //
        // 🎓 TUPLE KEY: (BuyerCode, Order, TypeCode, StyleCode)
        // This is the composite key that links StyleMaterialCostProfile to Style.
        // C# value tuples have correct equality semantics, so they work as
        // dictionary keys — two tuples with the same values are considered equal.
        var allMaterials = await dbContext.StyleMaterialCostProfiles
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        var materialsByStyle = allMaterials
            .GroupBy(m => (m.BuyerCode, m.Order, m.TypeCode, m.StyleCode))
            .ToDictionary(g => g.Key, g => g.ToList());

        _logger.LogInformation(
            "Loaded {MaterialCount} material cost profiles across {GroupCount} style groups",
            allMaterials.Count, materialsByStyle.Count);

        // ── Step 5: Chunk each style ────────────────────────────
        var allChunks = new List<EntityChunk>();

        foreach (var style in styles)
        {
            // Look up the buyer's display name from our dictionary.
            // If the BuyerCode doesn't exist (data integrity issue), fall back to "Buyer {code}".
            var buyerName = buyers.GetValueOrDefault(style.BuyerCode, $"Buyer {style.BuyerCode}");

            // Look up the garment type name (e.g., "T-Shirt", "Jacket").
            // TypeCode is the FK to GarmentTypes table.
            var garmentType = garmentTypes.GetValueOrDefault(style.TypeCode);

            // ── Build material dictionaries ─────────────
            // 🎓 The chunker expects IReadOnlyList<Dictionary<string, string>>
            // because it's decoupled from the data layer (doesn't reference
            // ApparelPro.Data entities). We build the dictionaries here in the
            // sync job, which IS the bridge between data and AI layers.
            var styleKey = (style.BuyerCode, style.Order, style.TypeCode, style.StyleCode);
            var styleMaterials = materialsByStyle.GetValueOrDefault(styleKey);

            List<Dictionary<string, string>>? materialDicts = null;
            if (styleMaterials is not null && styleMaterials.Count > 0)
            {
                materialDicts = styleMaterials.Select(m => new Dictionary<string, string>
                {
                    // Description is the material name (e.g., "Cotton Jersey 180GSM")
                    ["ItemName"] = m.Description ?? "Unknown material",

                    // Look up the supplier's real name instead of just showing a code number.
                    // 🎓 NOTE: StyleMaterialCostProfile.SupplierCode is a string (varchar(6)),
                    // but Supplier.SupplierCode is an int. We parse the string to int
                    // for the dictionary lookup. If parsing fails, we fall back to the raw code.
                    ["SupplierName"] = int.TryParse(m.SupplierCode, out var suppCode)
                            && suppliers.TryGetValue(suppCode, out var supplierName)
                        ? supplierName
                        : $"Supplier {m.SupplierCode}",

                    // TotalConsumption: how much of this material the style needs
                    ["Consumption"] = m.TotalConsumption.ToString("F2"),

                    // ItemUnit: the unit of measure (MTR, KG, PCS, etc.)
                    ["Unit"] = m.ItemUnit ?? "",

                    // UnitPrice: cost per unit of this material
                    ["UnitPrice"] = m.UnitPrice.ToString("F2")
                }).ToList();
            }

            // ── Call the chunker ────────────────────────
            // 🎓 ChunkStyle produces 1+ chunks depending on how many materials:
            //   - Style with 0-3 materials → typically 1 chunk
            //   - Style with 20+ materials → 2-3 chunks (materials overflow)
            // Each chunk includes the style header for context, so any single
            // chunk found during RAG search is self-contained.
            // 🎓 Pass both numeric codes (BuyerCode, TypeCode) alongside the display names.
            // The chunker includes them in the chunk text so Claude can extract
            // exact parameter values when detecting report intent — without these,
            // Claude guesses the IDs from names and gets them wrong.
            var chunks = chunker.ChunkStyle(
                styleCode: style.StyleCode,
                buyerCode: style.BuyerCode,
                buyerName: buyerName,
                order: style.Order,
                orderDate: style.OrderDate,
                typeCode: style.TypeCode,
                garmentType: garmentType,
                quantity: style.Quantity,
                unitPrice: style.UnitPrice,
                unit: style.Unit,
                materials: materialDicts);

            allChunks.AddRange(chunks);
        }

        _logger.LogInformation(
            "Chunked {StyleCount} styles into {ChunkCount} embedding chunks",
            styles.Count, allChunks.Count);

        return allChunks;
    }

    /// <summary>
    /// 🎓 PHASE C: REAL DATA FETCH — Purchase Orders
    ///
    /// Similar pattern to FetchStylesForSyncAsync, but for PurchaseOrders.
    ///
    /// Key differences:
    ///   - POs link to Styles as child records (one PO has many Styles)
    ///   - PurchaseOrder.OrderDate is DateTime (not DateOnly like Style.OrderDate)
    ///   - POs have additional fields: Season, BasisCode, BasisValue, Currency
    ///   - The ChunkPurchaseOrder method expects "styleLines" — a list of
    ///     styles belonging to this PO, as Dictionary&lt;string, string&gt;
    ///
    /// 🎓 THE PO↔STYLE RELATIONSHIP:
    /// In ApparelPro, a PurchaseOrder is identified by (BuyerCode, Order).
    /// Each Style belongs to a PO via the same (BuyerCode, Order) composite key.
    /// So to find all styles in PO "ORD-001" for Buyer 5, we filter:
    ///   styles.Where(s => s.BuyerCode == 5 && s.Order == "ORD-001")
    /// </summary>
    private async Task<List<EntityChunk>> FetchPurchaseOrdersForSyncAsync(
        IServiceScope scope,
        EntityChunkerService chunker,
        CancellationToken stoppingToken)
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApparelProDbContext>();

        // ── Load lookup tables ──────────────────────────
        var buyers = await dbContext.Buyers
            .AsNoTracking()
            .ToDictionaryAsync(b => b.BuyerCode, b => b.Name, stoppingToken);

        var garmentTypes = await dbContext.GarmentTypes
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.TypeName, stoppingToken);

        // ── Load all POs ────────────────────────────────
        var purchaseOrders = await dbContext.PurchaseOrders
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        _logger.LogInformation("Fetched {Count} purchase orders from database", purchaseOrders.Count);

        // ── Load styles grouped by PO (BuyerCode, Order) ──
        // 🎓 Each PO can have multiple styles. We group styles by their PO key
        // so we can include style line summaries in the PO chunk.
        // This enriches the PO vector — when someone asks "what styles are in PO X",
        // the PO chunk already contains that information.
        var allStyles = await dbContext.Styles
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        var stylesByPo = allStyles
            .GroupBy(s => (s.BuyerCode, s.Order))
            .ToDictionary(g => g.Key, g => g.ToList());

        // ── Chunk each PO ───────────────────────────────
        var allChunks = new List<EntityChunk>();

        foreach (var po in purchaseOrders)
        {
            var buyerName = buyers.GetValueOrDefault(po.BuyerCode, $"Buyer {po.BuyerCode}");

            // 🎓 PO.GarmentType is an int FK, not a string name.
            // Look up the display name from GarmentTypes table.
            var garmentTypeName = garmentTypes.GetValueOrDefault(po.GarmentType);

            // ── Build style line summaries ───────────────
            // 🎓 The chunker expects IReadOnlyList<Dictionary<string, string>>
            // with keys like "StyleCode", "Quantity", "UnitPrice".
            var poKey = (po.BuyerCode, po.Order);
            var poStyles = stylesByPo.GetValueOrDefault(poKey);

            List<Dictionary<string, string>>? styleLineDicts = null;
            if (poStyles is not null && poStyles.Count > 0)
            {
                styleLineDicts = poStyles.Select(s => new Dictionary<string, string>
                {
                    ["StyleCode"] = s.StyleCode,
                    ["Quantity"] = s.Quantity?.ToString("N0") ?? "0",
                    ["UnitPrice"] = s.UnitPrice?.ToString("F2") ?? "0.00"
                }).ToList();
            }

            // ── Call the chunker ────────────────────────
            var chunks = chunker.ChunkPurchaseOrder(
                order: po.Order,
                buyerName: buyerName,
                orderDate: po.OrderDate,     // DateTime (PO uses DateTime, not DateOnly)
                garmentTypeName: garmentTypeName,
                description: po.Description,
                countryCode: po.CountryCode,
                totalQuantity: po.TotalQuantity,
                unitCode: po.UnitCode,
                currencyCode: po.CurrencyCode,
                season: po.Season,
                basisCode: po.BasisCode,
                basisValue: po.BasisValue);

            allChunks.AddRange(chunks);
        }

        _logger.LogInformation(
            "Chunked {PoCount} purchase orders into {ChunkCount} embedding chunks",
            purchaseOrders.Count, allChunks.Count);

        return allChunks;
    }

    /// <summary>
    /// 🎓 PHASE C: REAL DATA FETCH — Buyers
    ///
    /// Buyers are simple reference entities — typically fit in a single chunk.
    /// But having buyer vectors in Qdrant means RAG can answer questions like:
    ///   "Tell me about buyer NEXT" → finds the buyer chunk
    ///   "Which buyer is in London?" → matches address content
    ///
    /// 🎓 THE ADDRESS JOIN:
    /// Buyer.Addresses is a navigation property (ICollection&lt;Address&gt;).
    /// But since the Address table links to Buyer via Address.BuyerCode,
    /// and we're not sure if Include() is configured in the EF model,
    /// we query addresses separately by BuyerCode — safe and explicit.
    ///
    /// 🎓 ADDRESS → DISPLAY STRING:
    /// The Address entity has no ToString() override. We manually concatenate
    /// StreetAddress, City, State, PostCode, CountryCode into a readable line.
    /// </summary>
    private async Task<List<EntityChunk>> FetchBuyersForSyncAsync(
        IServiceScope scope,
        EntityChunkerService chunker,
        CancellationToken stoppingToken)
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApparelProDbContext>();

        // ── Load all buyers ─────────────────────────────
        var buyerList = await dbContext.Buyers
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        _logger.LogInformation("Fetched {Count} buyers from database", buyerList.Count);

        // ── Load all addresses and group by BuyerCode ───
        // 🎓 Address.BuyerCode is the FK that links an address to a buyer.
        // Not all addresses belong to buyers (some have BankCode instead),
        // so we filter for non-null BuyerCode.
        var allAddresses = await dbContext.Addresses
            .AsNoTracking()
            .Where(a => a.BuyerCode != null)
            .ToListAsync(stoppingToken);

        var addressesByBuyer = allAddresses
            .GroupBy(a => a.BuyerCode!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ── Chunk each buyer ────────────────────────────
        var allChunks = new List<EntityChunk>();

        foreach (var buyer in buyerList)
        {
            // ── Build address display strings ───────────
            // 🎓 We build a human-readable address line from the components.
            // Example: "42 King Street, London, SE1 7PB, UK"
            // We skip empty/null components to avoid ugly ", , ," in the output.
            List<string>? addressStrings = null;
            var buyerAddresses = addressesByBuyer.GetValueOrDefault(buyer.BuyerCode);

            if (buyerAddresses is not null && buyerAddresses.Count > 0)
            {
                addressStrings = buyerAddresses.Select(a =>
                {
                    var parts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(a.StreetAddress)) parts.Add(a.StreetAddress);
                    if (!string.IsNullOrWhiteSpace(a.City)) parts.Add(a.City);
                    if (!string.IsNullOrWhiteSpace(a.State)) parts.Add(a.State);
                    if (a.PostCode.HasValue) parts.Add(a.PostCode.Value.ToString());
                    if (!string.IsNullOrWhiteSpace(a.CountryCode)) parts.Add(a.CountryCode);
                    return string.Join(", ", parts);
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            }

            // ── Call the chunker ────────────────────────
            var chunks = chunker.ChunkBuyer(
                buyerCode: buyer.BuyerCode,
                name: buyer.Name,
                status: buyer.Status,
                telephoneNos: buyer.TelephoneNos,
                mobileNos: buyer.MobileNos,
                addresses: addressStrings);

            allChunks.AddRange(chunks);
        }

        _logger.LogInformation(
            "Chunked {BuyerCount} buyers into {ChunkCount} embedding chunks",
            buyerList.Count, allChunks.Count);

        return allChunks;
    }

    /// <summary>
    /// 🎓 PHASE C: REAL DATA FETCH — Suppliers
    ///
    /// Similar to Buyers, but the address relationship is different:
    ///   - Buyer → Address: linked via Address.BuyerCode (one-to-many)
    ///   - Supplier → Address: linked via Supplier.AddressId (Guid?) matching
    ///     Address.AddressId (one-to-one or one-to-few)
    ///
    /// 🎓 WHY ARE SUPPLIER ADDRESSES DIFFERENT?
    /// The ApparelPro data model evolved from a Clipper legacy system.
    /// Buyers got the "modern" FK pattern (Address.BuyerCode), while
    /// Suppliers still use the older GUID-based link. Both work fine —
    /// they're just different historical patterns in the same codebase.
    ///
    /// Since Supplier.Addresses is [NotMapped], we query the Address table
    /// by matching Supplier.AddressId to Address.AddressId.
    /// </summary>
    private async Task<List<EntityChunk>> FetchSuppliersForSyncAsync(
        IServiceScope scope,
        EntityChunkerService chunker,
        CancellationToken stoppingToken)
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApparelProDbContext>();

        // ── Load all suppliers ───────────────────────────
        var supplierList = await dbContext.Suppliers
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        _logger.LogInformation("Fetched {Count} suppliers from database", supplierList.Count);

        // ── Load addresses for suppliers that have an AddressId ──
        // 🎓 Not all suppliers have addresses (AddressId is nullable Guid).
        // We load only those addresses whose AddressId matches a supplier's.
        var supplierAddressIds = supplierList
            .Where(s => s.AddressId.HasValue)
            .Select(s => s.AddressId!.Value)
            .ToHashSet();

        var supplierAddresses = supplierAddressIds.Count > 0
            ? await dbContext.Addresses
                .AsNoTracking()
                .Where(a => supplierAddressIds.Contains(a.AddressId))
                .ToListAsync(stoppingToken)
            : new List<ApparelPro.Data.Models.References.Address>();

        // Build a lookup: Supplier.AddressId → list of address display strings
        var addressByGuid = supplierAddresses
            .GroupBy(a => a.AddressId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ── Chunk each supplier ─────────────────────────
        var allChunks = new List<EntityChunk>();

        foreach (var supplier in supplierList)
        {
            // ── Build address display strings ───────────
            List<string>? addressStrings = null;

            if (supplier.AddressId.HasValue &&
                addressByGuid.TryGetValue(supplier.AddressId.Value, out var addrs) &&
                addrs.Count > 0)
            {
                addressStrings = addrs.Select(a =>
                {
                    var parts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(a.StreetAddress)) parts.Add(a.StreetAddress);
                    if (!string.IsNullOrWhiteSpace(a.City)) parts.Add(a.City);
                    if (!string.IsNullOrWhiteSpace(a.State)) parts.Add(a.State);
                    if (a.PostCode.HasValue) parts.Add(a.PostCode.Value.ToString());
                    if (!string.IsNullOrWhiteSpace(a.CountryCode)) parts.Add(a.CountryCode);
                    return string.Join(", ", parts);
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            }

            // ── Call the chunker ────────────────────────
            var chunks = chunker.ChunkSupplier(
                supplierCode: supplier.SupplierCode,
                name: supplier.Name,
                telephoneNos: supplier.TelephoneNos,
                mobileNos: supplier.MobileNos,
                addresses: addressStrings);

            allChunks.AddRange(chunks);
        }

        _logger.LogInformation(
            "Chunked {SupplierCount} suppliers into {ChunkCount} embedding chunks",
            supplierList.Count, allChunks.Count);

        return allChunks;
    }

    /// <summary>
    /// 🎓 PHASE 2 STEP 6: REAL DATA FETCH — Standard Operating Procedures (SOPs)
    ///
    /// This method brings company rules and procedures into the RAG pipeline.
    /// When a user asks "what are the quality requirements for buyer NEXT?" or
    /// "what procedures apply to trim sheet orders?", the SOP chunks in Qdrant
    /// will surface relevant procedures alongside the order/style data.
    ///
    /// 🎓 WHAT WE LOAD:
    /// 1. All StandardOperatingProcedures (active AND inactive — we include
    ///    the IsActive flag in the chunk text so Claude knows the status,
    ///    but inactive SOPs are still searchable for historical reference).
    /// 2. All SopApplicabilities — the linking rules that say WHERE each SOP
    ///    applies (which report types, buyers, suppliers, or globally).
    ///
    /// 🎓 WHY INCLUDE INACTIVE SOPs?
    /// Unlike the PDF injection pipeline (which strictly filters for active,
    /// date-valid SOPs), the RAG pipeline benefits from including all SOPs:
    ///   - Users may ask "what was the old packaging policy?"
    ///   - Claude can distinguish current vs expired policies from the metadata
    ///   - The chunk text clearly states "Status: Inactive" or "Effective until: 2024-01-01"
    ///
    /// 🎓 APPLICABILITY RULES AS CHUNK CONTEXT:
    /// Each SOP's applicability rules are converted to Dictionary&lt;string, string&gt;
    /// and passed to ChunkSop, which includes them in the header text.
    /// This means a search for "buyer 101 procedures" matches chunks whose
    /// header contains "Applies to: Buyer = 101" — much better retrieval
    /// than if we only embedded the procedure text itself.
    ///
    /// 🎓 NAVIGATION PROPERTY APPROACH:
    /// Unlike Styles (which lack navigation properties to CostProfiles),
    /// StandardOperatingProcedure HAS a navigation property to SopApplicabilities.
    /// We use .Include() to eager-load applicability rules in a single query
    /// rather than loading them separately and joining in memory.
    /// </summary>
    private async Task<List<EntityChunk>> FetchSopsForSyncAsync(
        IServiceScope scope,
        EntityChunkerService chunker,
        CancellationToken stoppingToken)
    {
        // ── Step 1: Resolve DbContext from the scoped provider ──
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApparelProDbContext>();

        // ── Step 2: Load all SOPs with their applicability rules ──
        // 🎓 .Include(s => s.SopApplicabilities) eager-loads the child rows
        // in a single query (LEFT JOIN). This is efficient here because:
        //   - SOP count is typically small (tens, not thousands)
        //   - Each SOP has a handful of applicability rules (not hundreds)
        //   - We need the rules for EVERY SOP anyway (no filtering)
        //
        // 🎓 AsNoTracking(): Read-only — we never modify SOPs in the sync job.
        var sops = await dbContext.StandardOperatingProcedures
            .AsNoTracking()
            .Include(s => s.SopApplicabilities)
            .ToListAsync(stoppingToken);

        _logger.LogInformation("Fetched {Count} SOPs from database", sops.Count);

        // ── Step 3: Chunk each SOP ─────────────────────────────
        var allChunks = new List<EntityChunk>();

        foreach (var sop in sops)
        {
            // ── Build applicability rule dictionaries ────
            // 🎓 The chunker expects IReadOnlyList<Dictionary<string, string>>
            // because it's decoupled from the data layer. We convert the
            // SopApplicability entities into simple string dictionaries here.
            //
            // Each dictionary has:
            //   "Type": the ApplicabilityType (e.g., "ReportType", "Buyer", "All")
            //   "Key": the ApplicabilityKey (e.g., "TrimSheet", "101", "*")
            //   "IsExcluded": "true" or "false" — whether this is an exclusion override
            List<Dictionary<string, string>>? applicabilityDicts = null;

            if (sop.SopApplicabilities.Count > 0)
            {
                applicabilityDicts = sop.SopApplicabilities.Select(sa => new Dictionary<string, string>
                {
                    ["Type"] = sa.ApplicabilityType,
                    ["Key"] = sa.ApplicabilityKey,
                    ["IsExcluded"] = sa.IsExcluded.ToString().ToLowerInvariant()
                }).ToList();
            }

            // ── Call the chunker ────────────────────────
            // 🎓 ChunkSop produces 1+ chunks depending on FullText length:
            //   - Short SOP (1 paragraph) → typically 1 chunk
            //   - Long SOP (multi-page procedure) → multiple chunks with header repeated
            //
            // The entityKey is SopId (int → string), so the Qdrant point ID becomes:
            //   "sop-42-chunk-0" — deterministic, so re-indexing overwrites correctly.
            var chunks = chunker.ChunkSop(
                sopId: sop.SopId,
                sopCode: sop.SopCode,
                title: sop.Title,
                description: sop.Description,
                fullText: sop.FullText,
                category: sop.Category,
                isActive: sop.IsActive,
                effectiveFrom: sop.EffectiveFrom,
                effectiveTo: sop.EffectiveTo,
                applicabilityRules: applicabilityDicts);

            allChunks.AddRange(chunks);
        }

        _logger.LogInformation(
            "Chunked {SopCount} SOPs into {ChunkCount} embedding chunks",
            sops.Count, allChunks.Count);

        return allChunks;
    }
}
