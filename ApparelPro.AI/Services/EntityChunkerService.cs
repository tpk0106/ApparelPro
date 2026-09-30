using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Configuration;

namespace ApparelPro.AI.Services;

/// <summary>
/// Converts ERP entities into text chunks suitable for embedding.
///
/// 🎓 WHAT IS CHUNKING?
/// Chunking is the process of splitting large text into smaller, overlapping pieces
/// before converting them to vectors (embeddings).
///
/// 🎓 WHY DO WE NEED CHUNKING?
/// Three reasons:
///
/// 1. EMBEDDING MODEL LIMITS:
///    Embedding models have token limits (text-embedding-3-small: 8191 tokens).
///    A style record with 50 material rows could easily exceed that.
///
/// 2. EMBEDDING QUALITY:
///    Long texts produce "diluted" embeddings — the vector tries to capture
///    everything and ends up representing nothing well. Short, focused chunks
///    produce sharper embeddings that match relevant queries more accurately.
///
/// 3. RETRIEVAL PRECISION:
///    If the whole style record is one vector, a query about "cotton fabric"
///    returns the entire record — including irrelevant shipping details.
///    With chunks, we return only the chunk about fabrics. Less noise = better answers.
///
/// 🎓 THE CHUNKING STRATEGY FOR APPARELPRO:
///
/// For ERP entities, we DON'T use naive character-based splitting (split every N chars).
/// Instead, we use SEMANTIC chunking — we understand the entity structure and split
/// at meaningful boundaries:
///
///   Style entity → produces chunks like:
///     Chunk 0: "Style S001 for Buyer NEXT. Order ORD-2024-001, dated 2024-01-15.
///               Type: T-Shirt. Quantity: 5000 pcs at $4.50/pc. Unit: PCS."
///     Chunk 1: "Style S001 Materials: Cotton Jersey 180GSM from Supplier 105,
///               consumption 1.2m per garment. Thread 40/2 from Supplier 210..."
///     Chunk 2: "Style S001 Materials (continued): Zipper YKK #5 from Supplier 088..."
///
/// Notice:
///   - Each chunk starts with entity context ("Style S001") so it makes sense alone
///   - We split at logical boundaries (header info, then materials)
///   - Overlapping context ensures nothing is lost between chunks
///
/// 🎓 TOKEN ESTIMATION:
/// We estimate ~4 characters per token (the standard English approximation).
/// So ChunkSize=500 tokens ≈ 2000 characters per chunk.
/// This is approximate — the actual tokeniser may vary, but for chunking
/// purposes this is accurate enough.
/// </summary>
public sealed class EntityChunkerService
{
    private readonly RagSettings _ragSettings;
    private readonly ILogger<EntityChunkerService> _logger;

    /// <summary>
    /// Approximate characters per token for estimation.
    /// English text averages ~4 characters per token with most tokenisers.
    /// </summary>
    private const int CharsPerToken = 4;

    public EntityChunkerService(
        IOptions<RagSettings> ragSettings,
        ILogger<EntityChunkerService> logger)
    {
        _ragSettings = ragSettings.Value;
        _logger = logger;
    }

    // ── Public chunking methods (one per entity type) ────

    /// <summary>
    /// Convert a Style entity (with its related data) into embeddable text chunks.
    ///
    /// 🎓 WHY ENTITY-SPECIFIC METHODS?
    /// Each entity type has different fields and different "important" information.
    /// A Style cares about buyer, garment type, quantity, and materials.
    /// A PurchaseOrder cares about delivery dates, country, currency, and basis.
    ///
    /// We could use generic reflection-based chunking, but explicit methods:
    ///   1. Produce BETTER text (we control the narrative)
    ///   2. Are EASIER to debug (you can read what each chunk says)
    ///   3. Let us PRIORITISE fields (put buyer name first, not database ID)
    ///
    /// 🎓 PARAMETERS:
    /// We pass dictionaries of related data (materials, events) as key-value pairs
    /// rather than requiring the exact EF Core entities. This decouples the chunker
    /// from the data layer — it doesn't need a reference to ApparelPro.Data.
    /// </summary>
    /// <param name="styleCode">The style's primary key (e.g., "S001").</param>
    /// <param name="buyerCode">
    /// 🆕 The buyer's numeric primary key (e.g., 1).
    /// 🎓 WHY INCLUDE THE NUMERIC CODE?
    /// Report generation endpoints need numeric IDs, not display names.
    /// For example, the Trim Sheet endpoint expects: ?buyerCode=1&amp;styleCode=ANCHORAGE
    /// Without this code in the chunk text, Claude has to GUESS the numeric ID
    /// from the buyer name — and gets it wrong (e.g., guessing buyerCode=5 for
    /// "LONDON FOG INDUSTRIES INC." when it's actually 1).
    /// By including "BuyerCode: 1" in the chunk, Claude can extract it accurately.
    /// </param>
    /// <param name="buyerName">The buyer's display name (e.g., "NEXT").</param>
    /// <param name="order">The order reference (e.g., "ORD-2024-001").</param>
    /// <param name="orderDate">When the order was placed.</param>
    /// <param name="typeCode">
    /// 🆕 The garment type's numeric primary key (e.g., 2 for "Mens Jacket").
    /// Same reasoning as buyerCode — report endpoints need the numeric ID.
    /// </param>
    /// <param name="garmentType">Type of garment (e.g., "T-Shirt").</param>
    /// <param name="quantity">Order quantity.</param>
    /// <param name="unitPrice">Price per unit.</param>
    /// <param name="unit">Unit of measure (e.g., "PCS").</param>
    /// <param name="materials">
    /// List of material consumption records, each as a dictionary:
    ///   { "ItemName": "Cotton Jersey", "SupplierName": "Fabric Co Ltd",
    ///     "Consumption": "1.2", "Unit": "MTR", "UnitPrice": "2.50" }
    /// </param>
    /// <param name="additionalContext">
    /// Any extra text to include (e.g., production notes, approval status).
    /// </param>
    /// <returns>List of text chunks, each within the configured token limit.</returns>
    public List<EntityChunk> ChunkStyle(
        string styleCode,
        int buyerCode,
        string buyerName,
        string order,
        DateOnly orderDate,
        int typeCode,
        string? garmentType,
        decimal? quantity,
        decimal? unitPrice,
        string? unit,
        IReadOnlyList<Dictionary<string, string>>? materials = null,
        string? additionalContext = null)
    {
        // ── Build the header ─────────────────────────
        // 🎓 THE HEADER: Always the first thing in every chunk.
        // It provides context so each chunk is self-contained.
        // If someone reads chunk 2 without chunk 0, they still know
        // which style this is about.
        //
        // 🎓 WHY INCLUDE NUMERIC CODES (BuyerCode, TypeCode)?
        // When Claude detects a report request, it needs to extract the exact
        // parameter values for the report endpoint. Without the numeric codes
        // in the chunk text, Claude can only see the display names ("LONDON FOG
        // INDUSTRIES INC.", "Mens Jacket") and has to GUESS the numeric IDs —
        // which it gets wrong. Including "BuyerCode: 1" and "TypeCode: 2"
        // gives Claude the exact values to extract for report parameters.
        var header = new StringBuilder();
        header.AppendLine($"Style {styleCode} for Buyer {buyerName} (BuyerCode: {buyerCode}).");
        header.AppendLine($"Order: {order}, dated {orderDate:yyyy-MM-dd}.");

        // 🎓 Include both the garment type NAME and its numeric CODE.
        // The name is for human readability in the RAG answer text.
        // The code is for Claude to extract when building report parameters.
        if (!string.IsNullOrWhiteSpace(garmentType))
            header.AppendLine($"Garment type: {garmentType} (TypeCode: {typeCode}).");
        else
            header.AppendLine($"TypeCode: {typeCode}.");

        if (quantity.HasValue)
        {
            var priceInfo = unitPrice.HasValue ? $" at {unitPrice:F2}/{unit ?? "pc"}" : "";
            header.AppendLine($"Quantity: {quantity:N0} {unit ?? "pcs"}{priceInfo}.");
        }

        if (!string.IsNullOrWhiteSpace(additionalContext))
            header.AppendLine(additionalContext);

        var headerText = header.ToString().TrimEnd();

        // ── Build material sections ──────────────────
        // 🎓 MATERIALS: The most detailed part of a style record.
        // Each material row gets its own line of natural-language text.
        // We group them into chunks that fit within the token limit.
        var materialLines = new List<string>();
        if (materials is not null && materials.Count > 0)
        {
            foreach (var mat in materials)
            {
                var line = new StringBuilder();
                line.Append($"Material: {mat.GetValueOrDefault("ItemName", "Unknown")}");

                if (mat.TryGetValue("SupplierName", out var supplier) && !string.IsNullOrWhiteSpace(supplier))
                    line.Append($" from Supplier {supplier}");

                if (mat.TryGetValue("Consumption", out var consumption))
                    line.Append($", consumption {consumption}");

                if (mat.TryGetValue("Unit", out var matUnit))
                    line.Append($" {matUnit}");

                if (mat.TryGetValue("UnitPrice", out var price))
                    line.Append($" at {price}/unit");

                line.Append('.');
                materialLines.Add(line.ToString());
            }
        }

        // ── Assemble chunks ──────────────────────────
        return BuildChunks(
            entityType: "Style",
            entityKey: styleCode,
            headerText: headerText,
            detailLines: materialLines,
            detailSectionTitle: "Materials");
    }

    /// <summary>
    /// Convert a PurchaseOrder entity into embeddable text chunks.
    ///
    /// 🎓 PO CHUNKING:
    /// Purchase Orders are typically shorter than Styles (fewer child records),
    /// so they often fit in a single chunk. But we use the same chunking framework
    /// for consistency, and in case a PO has many style lines.
    /// </summary>
    public List<EntityChunk> ChunkPurchaseOrder(
        string order,
        string buyerName,
        DateTime orderDate,
        string? garmentTypeName,
        string? description,
        string? countryCode,
        decimal totalQuantity,
        string? unitCode,
        string? currencyCode,
        string? season,
        string? basisCode,
        decimal basisValue,
        IReadOnlyList<Dictionary<string, string>>? styleLines = null)
    {
        var header = new StringBuilder();
        header.AppendLine($"Purchase Order {order} for Buyer {buyerName}.");
        header.AppendLine($"Order date: {orderDate:yyyy-MM-dd}.");

        if (!string.IsNullOrWhiteSpace(description))
            header.AppendLine($"Description: {description}.");

        if (!string.IsNullOrWhiteSpace(garmentTypeName))
            header.AppendLine($"Garment type: {garmentTypeName}.");

        header.AppendLine($"Total quantity: {totalQuantity:N0} {unitCode ?? "pcs"}.");

        if (!string.IsNullOrWhiteSpace(currencyCode))
            header.AppendLine($"Currency: {currencyCode}.");

        if (!string.IsNullOrWhiteSpace(season))
            header.AppendLine($"Season: {season}.");

        if (!string.IsNullOrWhiteSpace(basisCode))
            header.AppendLine($"Basis: {basisCode} ({basisValue:F2}).");

        if (!string.IsNullOrWhiteSpace(countryCode))
            header.AppendLine($"Country: {countryCode}.");

        var headerText = header.ToString().TrimEnd();

        // Style lines within the PO
        var styleLineTexts = new List<string>();
        if (styleLines is not null)
        {
            foreach (var sl in styleLines)
            {
                var line = new StringBuilder();
                line.Append($"Style {sl.GetValueOrDefault("StyleCode", "?")}");

                if (sl.TryGetValue("Quantity", out var qty))
                    line.Append($", Qty: {qty}");

                if (sl.TryGetValue("UnitPrice", out var price))
                    line.Append($" at {price}");

                line.Append('.');
                styleLineTexts.Add(line.ToString());
            }
        }

        return BuildChunks(
            entityType: "PurchaseOrder",
            entityKey: order,
            headerText: headerText,
            detailLines: styleLineTexts,
            detailSectionTitle: "Styles in this order");
    }

    /// <summary>
    /// Convert a Buyer entity into embeddable text chunks.
    ///
    /// 🎓 BUYERS: Usually small — fits in one chunk.
    /// But having a buyer chunk in the vector store means when someone asks
    /// "Tell me about buyer NEXT", RAG can pull in buyer details alongside
    /// any styles and POs for NEXT.
    /// </summary>
    public List<EntityChunk> ChunkBuyer(
        int buyerCode,
        string name,
        string? status,
        string? telephoneNos,
        string? mobileNos,
        IReadOnlyList<string>? addresses = null)
    {
        var header = new StringBuilder();
        header.AppendLine($"Buyer {name} (Code: {buyerCode}).");

        if (!string.IsNullOrWhiteSpace(status))
            header.AppendLine($"Status: {status}.");

        if (!string.IsNullOrWhiteSpace(telephoneNos))
            header.AppendLine($"Phone: {telephoneNos}.");

        if (!string.IsNullOrWhiteSpace(mobileNos))
            header.AppendLine($"Mobile: {mobileNos}.");

        var headerText = header.ToString().TrimEnd();

        var addressLines = addresses?
            .Select(a => $"Address: {a}.")
            .ToList() ?? new List<string>();

        return BuildChunks(
            entityType: "Buyer",
            entityKey: buyerCode.ToString(),
            headerText: headerText,
            detailLines: addressLines,
            detailSectionTitle: "Addresses");
    }

    /// <summary>
    /// Convert a Supplier entity into embeddable text chunks.
    /// </summary>
    public List<EntityChunk> ChunkSupplier(
        int supplierCode,
        string name,
        string? telephoneNos,
        string? mobileNos,
        IReadOnlyList<string>? addresses = null)
    {
        var header = new StringBuilder();
        header.AppendLine($"Supplier {name} (Code: {supplierCode}).");

        if (!string.IsNullOrWhiteSpace(telephoneNos))
            header.AppendLine($"Phone: {telephoneNos}.");

        if (!string.IsNullOrWhiteSpace(mobileNos))
            header.AppendLine($"Mobile: {mobileNos}.");

        var headerText = header.ToString().TrimEnd();

        var addressLines = addresses?
            .Select(a => $"Address: {a}.")
            .ToList() ?? new List<string>();

        return BuildChunks(
            entityType: "Supplier",
            entityKey: supplierCode.ToString(),
            headerText: headerText,
            detailLines: addressLines,
            detailSectionTitle: "Addresses");
    }

    /// <summary>
    /// Generic text chunker for any entity type not explicitly handled above.
    ///
    /// 🎓 EXTENSIBILITY:
    /// When you add a new entity to RAG (e.g., StockItem, Department),
    /// you can either:
    ///   1. Add a dedicated ChunkXxx method (best for complex entities)
    ///   2. Use this generic method (fine for simple entities)
    ///
    /// The generic method just takes pre-formatted text and splits it
    /// into overlapping chunks.
    /// </summary>
    /// <param name="entityType">The entity type name (e.g., "StockItem").</param>
    /// <param name="entityKey">The entity's primary key.</param>
    /// <param name="fullText">The full text representation of the entity.</param>
    /// <returns>List of text chunks with overlap.</returns>
    public List<EntityChunk> ChunkGenericText(
        string entityType,
        string entityKey,
        string fullText)
    {
        if (string.IsNullOrWhiteSpace(fullText))
            return new List<EntityChunk>();

        var maxChars = _ragSettings.ChunkSize * CharsPerToken;
        var overlapChars = _ragSettings.ChunkOverlap * CharsPerToken;

        // If the text fits in one chunk, return it directly
        if (fullText.Length <= maxChars)
        {
            return new List<EntityChunk>
            {
                new()
                {
                    EntityType = entityType,
                    EntityKey = entityKey,
                    ChunkIndex = 0,
                    Text = fullText,
                    TotalChunks = 1
                }
            };
        }

        // ── Split with overlap ───────────────────────
        // 🎓 OVERLAPPING WINDOW:
        // We slide a window across the text:
        //   Window 1: chars 0–2000
        //   Window 2: chars 1800–3800  (200 char overlap)
        //   Window 3: chars 3600–5600  (200 char overlap)
        //
        // The overlap ensures sentences aren't cut in half.
        // If a sentence spans the boundary, at least one chunk has it complete.
        var chunks = new List<EntityChunk>();
        var position = 0;
        var chunkIndex = 0;

        while (position < fullText.Length)
        {
            var length = Math.Min(maxChars, fullText.Length - position);
            var chunkText = fullText.Substring(position, length);

            // Try to break at a sentence boundary (period, newline)
            if (position + length < fullText.Length && length == maxChars)
            {
                var lastBreak = chunkText.LastIndexOfAny(new[] { '.', '\n', '!', '?' });
                if (lastBreak > maxChars / 2) // Don't break too early
                {
                    chunkText = chunkText[..(lastBreak + 1)];
                    length = chunkText.Length;
                }
            }

            chunks.Add(new EntityChunk
            {
                EntityType = entityType,
                EntityKey = entityKey,
                ChunkIndex = chunkIndex,
                Text = chunkText.Trim(),
                TotalChunks = 0 // Will be set below
            });

            position += length - overlapChars;
            if (position <= 0) position = length; // Safety: prevent infinite loop
            chunkIndex++;
        }

        // Set total chunk count on all chunks
        foreach (var chunk in chunks)
            chunk.TotalChunks = chunks.Count;

        return chunks;
    }

    // ── Private chunk builder ────────────────────────────

    /// <summary>
    /// 🎓 THE CORE CHUNKING ENGINE:
    /// Takes a header (always included) and detail lines (materials, styles, addresses),
    /// and packs them into chunks that fit within the token limit.
    ///
    /// Algorithm:
    ///   1. Start with the header text
    ///   2. Add detail lines one by one
    ///   3. When adding the next line would exceed the limit → start a new chunk
    ///   4. Each new chunk starts with the header (for context) + "continued" marker
    ///
    /// This ensures every chunk is self-contained — a search result
    /// always identifies which entity it belongs to.
    /// </summary>
    private List<EntityChunk> BuildChunks(
        string entityType,
        string entityKey,
        string headerText,
        List<string> detailLines,
        string detailSectionTitle)
    {
        var maxChars = _ragSettings.ChunkSize * CharsPerToken;
        var chunks = new List<EntityChunk>();

        // If no detail lines, the whole entity fits in the header
        if (detailLines.Count == 0)
        {
            chunks.Add(new EntityChunk
            {
                EntityType = entityType,
                EntityKey = entityKey,
                ChunkIndex = 0,
                Text = headerText,
                TotalChunks = 1
            });

            _logger.LogDebug(
                "Chunked {EntityType}/{EntityKey}: 1 chunk ({Length} chars)",
                entityType, entityKey, headerText.Length);

            return chunks;
        }

        // ── Pack detail lines into chunks ────────────
        var currentChunk = new StringBuilder(headerText);
        currentChunk.AppendLine();
        currentChunk.AppendLine($"{detailSectionTitle}:");

        var chunkIndex = 0;

        foreach (var line in detailLines)
        {
            // Would adding this line exceed the limit?
            if (currentChunk.Length + line.Length + 2 > maxChars && currentChunk.Length > headerText.Length + 20)
            {
                // Save current chunk
                chunks.Add(new EntityChunk
                {
                    EntityType = entityType,
                    EntityKey = entityKey,
                    ChunkIndex = chunkIndex,
                    Text = currentChunk.ToString().TrimEnd(),
                    TotalChunks = 0 // Set after all chunks are built
                });

                chunkIndex++;

                // Start new chunk with header context
                currentChunk.Clear();
                currentChunk.AppendLine(headerText);
                currentChunk.AppendLine($"{detailSectionTitle} (continued):");
            }

            currentChunk.AppendLine(line);
        }

        // Don't forget the last chunk
        if (currentChunk.Length > 0)
        {
            chunks.Add(new EntityChunk
            {
                EntityType = entityType,
                EntityKey = entityKey,
                ChunkIndex = chunkIndex,
                Text = currentChunk.ToString().TrimEnd(),
                TotalChunks = 0
            });
        }

        // Set total chunk count
        foreach (var chunk in chunks)
            chunk.TotalChunks = chunks.Count;

        _logger.LogDebug(
            "Chunked {EntityType}/{EntityKey}: {Count} chunks",
            entityType, entityKey, chunks.Count);

        return chunks;
    }
}

// ── Supporting model ─────────────────────────────────────

/// <summary>
/// A single text chunk produced by the EntityChunkerService.
///
/// 🎓 LIFECYCLE OF AN EntityChunk:
///   1. EntityChunkerService creates it from entity data
///   2. EmbeddingService converts its Text to a vector
///   3. QdrantVectorStoreService stores the vector + metadata
///   4. On search, the Text is retrieved and sent to Claude as context
///
/// The Id property generates the Qdrant point ID automatically:
///   "style-S001-chunk-0" — deterministic, so re-indexing overwrites correctly.
/// </summary>
public sealed class EntityChunk
{
    /// <summary>The entity type (e.g., "Style", "PurchaseOrder").</summary>
    public required string EntityType { get; init; }

    /// <summary>The entity's primary key (e.g., "S001").</summary>
    public required string EntityKey { get; init; }

    /// <summary>The zero-based chunk index within this entity.</summary>
    public required int ChunkIndex { get; init; }

    /// <summary>The actual text content of this chunk (for embedding).</summary>
    public required string Text { get; set; }

    /// <summary>Total number of chunks for this entity.</summary>
    public int TotalChunks { get; set; }

    /// <summary>
    /// Generates the unique point ID for Qdrant storage.
    /// Format: "{entityType}-{entityKey}-chunk-{index}" (lowercase).
    /// Example: "style-S001-chunk-0"
    /// </summary>
    public string PointId =>
        $"{EntityType.ToLowerInvariant()}-{EntityKey}-chunk-{ChunkIndex}";
}
