using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;

namespace ApparelPro.AI.Services;

/// <summary>
/// Qdrant-based vector store implementation.
///
/// 🎓 WHAT IS QDRANT?
/// Qdrant (pronounced "quadrant") is an open-source vector database written in Rust.
/// It's designed for one thing: storing vectors and finding similar ones — fast.
///
/// Think of it like this:
///   SQL Server stores ROWS and finds exact matches (WHERE clause)
///   Qdrant stores VECTORS and finds similar meanings (cosine similarity)
///
/// 🎓 HOW WE CONNECT TO QDRANT:
/// Qdrant runs as a Docker container (see qdrant-docker-compose.yml).
/// It exposes two ports:
///   - 6333: REST API (HTTP) — simple, good for debugging
///   - 6334: gRPC — faster, used by this .NET client for production
///
/// The Qdrant .NET client (NuGet: Qdrant.Client) uses gRPC internally
/// for maximum performance — it's ~2-3× faster than REST for bulk operations.
///
/// 🎓 ARCHITECTURE NOTE:
/// This service implements IVectorStoreService, so the rest of the RAG pipeline
/// doesn't know or care that we're using Qdrant. If you later switch to
/// Azure AI Search, you'd create AzureVectorStoreService : IVectorStoreService
/// and swap the DI registration — zero changes to the RAG logic.
///
/// 🎓 NuGet PACKAGE NEEDED:
/// Add to ApparelPro.AI.csproj:
///   &lt;PackageReference Include="Qdrant.Client" Version="1.*" /&gt;
/// </summary>
public sealed class QdrantVectorStoreService : IVectorStoreService
{
    // ── Private fields ───────────────────────────────────

    private readonly QdrantClient _qdrantClient;
    private readonly RagSettings _ragSettings;
    private readonly ILogger<QdrantVectorStoreService> _logger;

    // ── Constructor ──────────────────────────────────────

    /// <summary>
    /// 🎓 QDRANT CLIENT SETUP:
    /// The QdrantClient connects to the Qdrant server via gRPC.
    /// We configure it with:
    ///   - Host and port from RagSettings.QdrantUrl
    ///   - Optional API key for production security
    ///
    /// The client is thread-safe and maintains a connection pool internally,
    /// so we create it once and reuse it for the app's lifetime.
    /// </summary>
    public QdrantVectorStoreService(
        IOptions<RagSettings> ragSettings,
        ILogger<QdrantVectorStoreService> logger)
    {
        _ragSettings = ragSettings.Value;
        _logger = logger;

        // ── Parse the Qdrant URL ─────────────────────
        // 🎓 WHY PARSE THE URL?
        // The QdrantClient constructor takes host and port separately.
        // In config, it's more natural to store "http://localhost:6333" as one value.
        // We parse it here so the config is clean.
        //
        // Note: The QdrantClient uses gRPC (port 6334) by default.
        // But Qdrant maps REST port 6333 → gRPC port 6334 internally,
        // so we can use either port. The SDK handles the protocol.
        var uri = new Uri(_ragSettings.QdrantUrl);

        if (!string.IsNullOrWhiteSpace(_ragSettings.QdrantApiKey))
        {
            // 🎓 PRODUCTION MODE: Connect with API key authentication
            // This prevents unauthorised access to your vector data.
            _qdrantClient = new QdrantClient(
                host: uri.Host,
                port: uri.Port,
                https: uri.Scheme == "https",
                apiKey: _ragSettings.QdrantApiKey);
        }
        else
        {
            // 🎓 DEVELOPMENT MODE: Connect without authentication
            // Fine for local Docker, but NOT for production!
            _qdrantClient = new QdrantClient(
                host: uri.Host,
                port: uri.Port,
                https: uri.Scheme == "https");
        }

        _logger.LogInformation(
            "QdrantVectorStoreService initialised. Host: {Host}, Collection: {Collection}",
            _ragSettings.QdrantUrl,
            _ragSettings.CollectionName);
    }

    // ── Ensure collection exists ─────────────────────────

    /// <summary>
    /// 🎓 COLLECTION = TABLE (in SQL terms)
    ///
    /// Before we can store any vectors, the collection must exist in Qdrant.
    /// This method is idempotent — calling it multiple times is safe.
    ///
    /// What it creates:
    ///   Collection: "apparelpro-entities"
    ///   Vector config:
    ///     - Size: 1536 (matching text-embedding-3-small)
    ///     - Distance: Cosine (the similarity metric)
    ///
    /// 🎓 WHY COSINE DISTANCE?
    /// There are three common distance metrics:
    ///   - Cosine: Measures the ANGLE between vectors (ignores magnitude)
    ///     → Best for text embeddings because we care about MEANING, not length
    ///   - Euclidean: Measures straight-line distance (affected by magnitude)
    ///   - Dot Product: Measures alignment × magnitude
    ///
    /// Cosine is the standard for text embeddings. Two chunks about "cotton fabric"
    /// will have similar direction in vector space even if one chunk is longer.
    /// </summary>
    public async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // ── Check if collection already exists ───
            var collections = await _qdrantClient.ListCollectionsAsync(cancellationToken);
            var exists = collections.Any(c => c == _ragSettings.CollectionName);

            if (exists)
            {
                _logger.LogInformation(
                    "Qdrant collection '{Collection}' already exists.",
                    _ragSettings.CollectionName);
                return;
            }

            // ── Create the collection ────────────────
            _logger.LogInformation(
                "Creating Qdrant collection '{Collection}' with {Dimensions} dimensions, Cosine distance",
                _ragSettings.CollectionName,
                _ragSettings.EmbeddingDimensions);

            await _qdrantClient.CreateCollectionAsync(
                collectionName: _ragSettings.CollectionName,
                vectorsConfig: new VectorParams
                {
                    Size = (ulong)_ragSettings.EmbeddingDimensions,
                    Distance = Distance.Cosine
                },
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Qdrant collection '{Collection}' created successfully.",
                _ragSettings.CollectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to ensure Qdrant collection '{Collection}' exists. " +
                "Is Qdrant running at {Url}?",
                _ragSettings.CollectionName,
                _ragSettings.QdrantUrl);
            throw;
        }
    }

    // ── Upsert vectors ───────────────────────────────────

    /// <summary>
    /// 🎓 UPSERT = "INSERT or UPDATE"
    ///
    /// This method takes our VectorPoint models and converts them to Qdrant's
    /// native PointStruct format, then sends them in one batch call.
    ///
    /// 🎓 QDRANT'S INTERNAL STRUCTURE:
    /// Each point in Qdrant has:
    ///   - Id: A unique identifier (we use PointId.FromString for named IDs)
    ///   - Vectors: The actual float array
    ///   - Payload: A dictionary of metadata (like JSON properties)
    ///
    /// 🎓 WHY PointId?
    /// Qdrant supports two types of IDs:
    ///   - UUID (random): Good for auto-generated IDs
    ///   - Named string hash: Our approach — deterministic from entity+chunk
    ///     "style-S001-chunk-0" always produces the same ID, so re-indexing
    ///     naturally overwrites the old vector (true upsert behavior).
    /// </summary>
    public async Task UpsertAsync(
        IReadOnlyList<VectorPoint> points,
        CancellationToken cancellationToken = default)
    {
        if (points.Count == 0) return;

        try
        {
            _logger.LogInformation(
                "Upserting {Count} points to Qdrant collection '{Collection}'",
                points.Count,
                _ragSettings.CollectionName);

            // ── Convert our models to Qdrant format ──
            // 🎓 TRANSFORMATION PIPELINE:
            //   VectorPoint (our model) → PointStruct (Qdrant's model)
            //
            // This is like mapping an API model to a database entity —
            // same data, different shape to match the target system's expectations.
            var qdrantPoints = points.Select(p =>
            {
                // Convert payload dictionary to Qdrant's Value type
                var payload = p.Payload.ToDictionary(
                    kvp => kvp.Key,
                    kvp => ConvertToQdrantValue(kvp.Value));

                return new PointStruct
                {
                    // 🎓 PointId.NewGuid() from a deterministic hash
                    // We hash our string ID to create a stable UUID.
                    // Same string → same UUID → upsert overwrites correctly.
                    Id = new PointId { Uuid = CreateDeterministicGuid(p.Id).ToString() },
                    Vectors = p.Vector.ToArray(),
                    Payload = { payload }
                };
            }).ToList();

            // ── Send to Qdrant ───────────────────────
            // 🎓 UpsertAsync sends all points in a single gRPC call.
            // Qdrant processes them atomically — either all succeed or none do.
            // For large batches (>1000 points), you might want to chunk them,
            // but for ApparelPro's scale this is fine.
            await _qdrantClient.UpsertAsync(
                collectionName: _ragSettings.CollectionName,
                points: qdrantPoints,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully upserted {Count} points to Qdrant",
                points.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to upsert {Count} points to Qdrant collection '{Collection}'",
                points.Count,
                _ragSettings.CollectionName);
            throw;
        }
    }

    // ── Search vectors ───────────────────────────────────

    /// <summary>
    /// 🎓 THE HEART OF RAG: Similarity Search
    ///
    /// This is where the magic happens. Given a query vector (the user's question
    /// converted to numbers), Qdrant finds the stored vectors closest to it.
    ///
    /// Step by step:
    ///   1. User asks: "Which styles use cotton fabric?"
    ///   2. EmbeddingService converts this to a 1536-float vector
    ///   3. This method sends that vector to Qdrant
    ///   4. Qdrant's HNSW index finds the nearest neighbours
    ///   5. Returns top K results with scores
    ///   6. We filter out anything below MinimumScore (noise)
    ///   7. Return the relevant chunks to the caller
    ///
    /// 🎓 OPTIONAL ENTITY TYPE FILTER:
    /// Sometimes the user's question is entity-specific:
    ///   "Summarise PO-2024-001" → only search PurchaseOrder chunks
    ///   "Which suppliers are late?" → search all entity types
    ///
    /// We use Qdrant's payload filter to narrow the search:
    ///   Filter: { "entityType": { "match": "PurchaseOrder" } }
    /// This is like adding a WHERE clause to a SQL query.
    /// </summary>
    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        ReadOnlyMemory<float> queryVector,
        int topK = 5,
        double minimumScore = 0.65,
        string? entityTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Searching Qdrant collection '{Collection}'. TopK: {TopK}, MinScore: {MinScore}, Filter: {Filter}",
                _ragSettings.CollectionName,
                topK,
                minimumScore,
                entityTypeFilter ?? "(all)");

            // ── Build optional filter ────────────────
            // 🎓 QDRANT FILTERS:
            // Filters work on payload fields (metadata), not on vectors.
            // They narrow down WHICH vectors to compare against, BEFORE
            // computing similarity. This is efficient because Qdrant
            // doesn't waste time comparing against irrelevant entity types.
            Filter? filter = null;
            if (!string.IsNullOrWhiteSpace(entityTypeFilter))
            {
                filter = new Filter
                {
                    Must =
                    {
                        new Condition
                        {
                            Field = new FieldCondition
                            {
                                Key = "entityType",
                                Match = new Match { Keyword = entityTypeFilter }
                            }
                        }
                    }
                };
            }

            // ── Execute the search ───────────────────
            // 🎓 SearchAsync parameters:
            //   - vector: What we're looking for (the question's embedding)
            //   - limit: Maximum results (TopK)
            //   - scoreThreshold: Minimum score (filters out noise)
            //   - filter: Optional payload filter (entity type)
            //   - payloadSelector: true = include all payload data in results
            //     (we need the chunkText to build context for Claude)
            var searchResults = await _qdrantClient.QueryAsync(
                collectionName: _ragSettings.CollectionName,
                query: queryVector.ToArray(),
                limit: (ulong)topK,
                scoreThreshold: (float)minimumScore,
                filter: filter,
                payloadSelector: true,
                cancellationToken: cancellationToken);

            // ── Convert Qdrant results to our model ──
            var results = searchResults
                .Select(r => new VectorSearchResult
                {
                    Score = r.Score,
                    Id = r.Id.Uuid ?? r.Id.Num.ToString(),
                    Payload = r.Payload.ToDictionary(
                        kvp => kvp.Key,
                        kvp => ConvertFromQdrantValue(kvp.Value))
                })
                .ToList();

            _logger.LogInformation(
                "Qdrant search returned {Count} results (of {TopK} requested)",
                results.Count,
                topK);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to search Qdrant collection '{Collection}'",
                _ragSettings.CollectionName);
            throw;
        }
    }

    // ── Delete by entity ─────────────────────────────────

    /// <summary>
    /// 🎓 CLEANUP: Remove all vectors for a specific entity.
    ///
    /// When a Style with 3 chunks is updated and now has 2 chunks,
    /// we need to delete the old 3 and insert the new 2. Without deletion,
    /// chunk-2 from the old version would remain as stale data.
    ///
    /// Qdrant's filter-based delete is efficient: it uses the payload index
    /// to find matching points, then removes them in one operation.
    /// </summary>
    public async Task DeleteByEntityAsync(
        string entityType,
        string entityKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Deleting vectors for entity {EntityType}/{EntityKey} from collection '{Collection}'",
                entityType,
                entityKey,
                _ragSettings.CollectionName);

            // ── Build a filter that matches both entityType AND entityKey ──
            // 🎓 MUST = AND logic: Both conditions must be true.
            // This ensures we only delete chunks for THIS specific entity,
            // not all Styles or all chunks with this key value.
            var filter = new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "entityType",
                            Match = new Match { Keyword = entityType }
                        }
                    },
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "entityKey",
                            Match = new Match { Keyword = entityKey }
                        }
                    }
                }
            };

            await _qdrantClient.DeleteAsync(
                collectionName: _ragSettings.CollectionName,
                filter: filter,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully deleted vectors for entity {EntityType}/{EntityKey}",
                entityType,
                entityKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to delete vectors for entity {EntityType}/{EntityKey}",
                entityType,
                entityKey);
            throw;
        }
    }

    // ── Get point count ──────────────────────────────────

    /// <summary>
    /// 🎓 MONITORING: How many vectors are in the store?
    ///
    /// Useful for:
    ///   - Health check endpoints
    ///   - Admin dashboards showing indexing progress
    ///   - Verifying the sync job is working
    /// </summary>
    public async Task<long> GetPointCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var info = await _qdrantClient.GetCollectionInfoAsync(
                _ragSettings.CollectionName,
                cancellationToken);

            var count = (long)info.PointsCount;

            _logger.LogDebug(
                "Qdrant collection '{Collection}' has {Count} points",
                _ragSettings.CollectionName,
                count);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get point count for Qdrant collection '{Collection}'",
                _ragSettings.CollectionName);
            throw;
        }
    }

    // ── Private helpers ──────────────────────────────────

    /// <summary>
    /// 🎓 DETERMINISTIC GUID FROM STRING:
    /// Converts a human-readable ID like "style-S001-chunk-0" into a UUID
    /// that Qdrant can use as a point ID.
    ///
    /// "Deterministic" means the SAME input ALWAYS produces the SAME output:
    ///   "style-S001-chunk-0" → always "a1b2c3d4-..." (same UUID every time)
    ///
    /// This is critical for upsert: when we re-index Style S001, the new vector
    /// gets the SAME UUID as the old one, so Qdrant replaces it instead of
    /// creating a duplicate.
    ///
    /// We use UUID v5 (SHA-1 based, namespace-deterministic) — the standard
    /// way to derive UUIDs from names.
    /// </summary>
    private static Guid CreateDeterministicGuid(string input)
    {
        // Use a fixed namespace UUID (RFC 4122 URL namespace)
        var namespaceId = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

        // Combine namespace bytes + input bytes
        var namespaceBytes = namespaceId.ToByteArray();
        // Swap byte order for RFC compliance
        SwapGuidByteOrder(namespaceBytes);

        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var combined = new byte[namespaceBytes.Length + inputBytes.Length];
        Buffer.BlockCopy(namespaceBytes, 0, combined, 0, namespaceBytes.Length);
        Buffer.BlockCopy(inputBytes, 0, combined, namespaceBytes.Length, inputBytes.Length);

        // SHA-1 hash
        var hash = System.Security.Cryptography.SHA1.HashData(combined);

        // Take first 16 bytes and set version (5) and variant bits
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50); // Version 5
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // Variant RFC 4122

        SwapGuidByteOrder(guidBytes);
        return new Guid(guidBytes);
    }

    /// <summary>
    /// 🎓 .NET's Guid stores bytes in mixed-endian format.
    /// We need to swap the first 3 groups for RFC 4122 compliance.
    /// </summary>
    private static void SwapGuidByteOrder(byte[] guid)
    {
        // Swap first 4 bytes
        (guid[0], guid[3]) = (guid[3], guid[0]);
        (guid[1], guid[2]) = (guid[2], guid[1]);
        // Swap bytes 4-5
        (guid[4], guid[5]) = (guid[5], guid[4]);
        // Swap bytes 6-7
        (guid[6], guid[7]) = (guid[7], guid[6]);
    }

    /// <summary>
    /// 🎓 TYPE CONVERSION: C# object → Qdrant Value
    ///
    /// Qdrant's gRPC protocol uses its own Value type (like JSON values).
    /// We need to convert our C# dictionary values to Qdrant's format.
    ///
    /// Supported types:
    ///   string  → StringValue
    ///   int/long → IntegerValue
    ///   double/float → DoubleValue
    ///   bool → BoolValue
    ///   anything else → StringValue via .ToString()
    /// </summary>
    private static Value ConvertToQdrantValue(object value)
    {
        return value switch
        {
            string s => new Value { StringValue = s },
            int i => new Value { IntegerValue = i },
            long l => new Value { IntegerValue = l },
            double d => new Value { DoubleValue = d },
            float f => new Value { DoubleValue = f },
            bool b => new Value { BoolValue = b },
            _ => new Value { StringValue = value?.ToString() ?? string.Empty }
        };
    }

    /// <summary>
    /// 🎓 TYPE CONVERSION: Qdrant Value → C# object
    ///
    /// The reverse of ConvertToQdrantValue: when we get search results back
    /// from Qdrant, we need to convert its Value type back to C# objects
    /// so the rest of our code can work with them naturally.
    /// </summary>
    private static object ConvertFromQdrantValue(Value value)
    {
        return value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.IntegerValue => value.IntegerValue,
            Value.KindOneofCase.DoubleValue => value.DoubleValue,
            Value.KindOneofCase.BoolValue => value.BoolValue,
            _ => value.StringValue ?? string.Empty
        };
    }
}
