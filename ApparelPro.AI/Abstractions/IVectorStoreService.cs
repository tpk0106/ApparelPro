namespace ApparelPro.AI.Abstractions;

/// <summary>
/// Abstraction for storing and searching vector embeddings.
///
/// 🎓 WHAT IS A VECTOR STORE?
/// A vector store is a specialised database designed for one job:
/// finding vectors that are "close" to a query vector — fast.
///
/// Regular databases (SQL Server) are great at exact matches:
///   SELECT * FROM Styles WHERE BuyerCode = 'NEXT'
///
/// Vector databases are great at SIMILARITY searches:
///   "Find the 5 vectors most similar to this query vector"
///   → These represent the 5 text chunks most relevant to the user's question
///
/// 🎓 WHY AN ABSTRACTION?
/// Right now we use Qdrant (free, open-source, runs in Docker).
/// But you might later want to switch to:
///   - Azure AI Search (managed, enterprise)
///   - Pinecone (cloud-native, serverless)
///   - Weaviate (open-source alternative)
///   - pgvector (PostgreSQL extension — use your existing DB!)
///
/// With this interface, swapping vector stores is a config change + new implementation,
/// no changes to the RAG pipeline itself.
///
/// 🎓 KEY CONCEPT: Points
/// In Qdrant terminology, each stored vector is a "point":
///   Point = {
///     Id:       "style-S001-chunk-0"         ← unique identifier
///     Vector:   [0.023, -0.041, ...]          ← the embedding (1536 floats)
///     Payload:  { entityType: "Style",        ← metadata for filtering
///                 entityKey: "S001",
///                 chunkText: "Cotton T-Shirt...",
///                 chunkIndex: 0 }
///   }
/// </summary>
public interface IVectorStoreService
{
    /// <summary>
    /// Ensure the vector collection exists, creating it if necessary.
    ///
    /// 🎓 WHEN IS THIS CALLED?
    /// At application startup (in DI registration or a hosted service).
    /// It's like running EF Core migrations — make sure the "table" exists
    /// before we try to insert data.
    ///
    /// 🎓 WHAT HAPPENS INSIDE (for Qdrant):
    /// 1. Check if collection "apparelpro-entities" exists
    /// 2. If not, create it with:
    ///    - Vector size: 1536 (matching our embedding model)
    ///    - Distance metric: Cosine (measures angle between vectors)
    ///    - HNSW index: Hierarchical Navigable Small World graph for fast search
    ///
    /// 🎓 WHAT IS HNSW?
    /// Instead of comparing the query vector against EVERY stored vector (slow!),
    /// HNSW builds a graph structure that lets Qdrant find approximate nearest
    /// neighbours in O(log n) time instead of O(n). For 100K vectors, this means
    /// ~17 comparisons instead of 100,000.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Store (upsert) one or more vectors with their metadata.
    ///
    /// 🎓 WHAT IS "UPSERT"?
    /// Upsert = Update + Insert:
    ///   - If a point with this ID doesn't exist → INSERT it
    ///   - If a point with this ID already exists → UPDATE it (replace vector + payload)
    ///
    /// This is important for the sync job: when a Style is modified, we re-embed it
    /// and upsert — no need to delete the old vector first.
    ///
    /// 🎓 WHAT IS VectorPoint?
    /// Our internal model for a single vector entry:
    ///   - Id: Unique string (e.g., "style-S001-chunk-0")
    ///   - Vector: The 1536-float embedding
    ///   - Payload: Key-value metadata for filtering and display
    /// </summary>
    /// <param name="points">The vector points to store or update.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task UpsertAsync(
        IReadOnlyList<VectorPoint> points,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for vectors most similar to a query vector.
    ///
    /// 🎓 THIS IS THE "R" IN RAG — Retrieval!
    /// When a user asks "Which styles use cotton fabric?":
    ///   1. EmbeddingService converts the question to a vector
    ///   2. This method searches Qdrant for the closest stored vectors
    ///   3. Returns the top K matches with their metadata (chunk text)
    ///   4. The RagService feeds those chunks to Claude as context
    ///
    /// 🎓 HOW SIMILARITY SEARCH WORKS:
    /// Qdrant computes cosine similarity between the query vector and every
    /// stored vector (using its HNSW index for speed). Results are ranked
    /// by score:
    ///   Score 0.95: "Cotton fabric for Style S001" ← very relevant
    ///   Score 0.82: "Polyester blend for Style S045" ← somewhat relevant
    ///   Score 0.55: "Shipping container tracking" ← not relevant (below threshold)
    /// </summary>
    /// <param name="queryVector">The embedding of the user's question.</param>
    /// <param name="topK">How many results to return (default from RagSettings.TopK).</param>
    /// <param name="minimumScore">Minimum similarity score to include (0.0–1.0).</param>
    /// <param name="entityTypeFilter">
    /// Optional: only search within a specific entity type.
    /// Example: "Style" → only returns style-related chunks.
    /// Null → searches all entity types.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A list of search results, ordered by relevance (highest score first).
    /// Each result includes the score, original text chunk, and metadata.
    /// </returns>
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        ReadOnlyMemory<float> queryVector,
        int topK = 5,
        double minimumScore = 0.65,
        string? entityTypeFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all vectors belonging to a specific entity.
    ///
    /// 🎓 WHEN IS THIS NEEDED?
    /// - When an entity is deleted from ApparelPro (e.g., a cancelled PO)
    /// - When re-indexing an entity: delete old chunks, then upsert new ones
    ///   (handles the case where the entity now has FEWER chunks than before)
    ///
    /// 🎓 HOW IT WORKS:
    /// We filter by the "entityType" and "entityKey" metadata fields,
    /// then delete all matching points. For example:
    ///   DeleteByEntityAsync("Style", "S001")
    /// would delete chunks: style-S001-chunk-0, style-S001-chunk-1, style-S001-chunk-2
    /// </summary>
    /// <param name="entityType">The entity type (e.g., "Style", "PurchaseOrder").</param>
    /// <param name="entityKey">The entity's primary key value.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteByEntityAsync(
        string entityType,
        string entityKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the total number of vectors stored in the collection.
    ///
    /// 🎓 WHY IS THIS USEFUL?
    /// - Health checks: "Is our vector store populated?"
    /// - Dashboard metrics: "How many chunks are indexed?"
    /// - Sync monitoring: "Did the sync job add the expected number of vectors?"
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The total number of stored vector points.</returns>
    Task<long> GetPointCountAsync(CancellationToken cancellationToken = default);
}

// ── Supporting models ────────────────────────────────────

/// <summary>
/// A single vector entry to store in the vector database.
///
/// 🎓 THINK OF THIS AS A "ROW" IN THE VECTOR DATABASE:
///   Id     = Primary key (unique identifier for this chunk)
///   Vector = The actual embedding (what gets compared during search)
///   Payload = Metadata (extra info we carry alongside the vector)
///
/// The Payload dictionary is flexible — we can add any key-value pairs.
/// For ApparelPro, we always include:
///   - "entityType": "Style" | "PurchaseOrder" | "Buyer" | etc.
///   - "entityKey": The entity's primary key (e.g., "S001")
///   - "chunkText": The original text that was embedded
///   - "chunkIndex": Which chunk number this is (0, 1, 2, ...)
///   - "updatedAt": ISO 8601 timestamp of when this was last indexed
/// </summary>
public sealed class VectorPoint
{
    /// <summary>
    /// Unique identifier for this point.
    /// Format: "{entityType}-{entityKey}-chunk-{index}"
    /// Example: "style-S001-chunk-0"
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The embedding vector (1536 floats for text-embedding-3-small).
    /// </summary>
    public required ReadOnlyMemory<float> Vector { get; init; }

    /// <summary>
    /// Key-value metadata stored alongside the vector.
    /// Used for filtering search results and retrieving original text.
    /// </summary>
    public required Dictionary<string, object> Payload { get; init; }
}

/// <summary>
/// A single result from a vector similarity search.
///
/// 🎓 WHAT THE USER GETS BACK:
/// When you search for "Which suppliers are late?", the vector store returns
/// a ranked list of these results. Each one says:
///   - Score: How relevant is this chunk? (0.0 to 1.0)
///   - Id: Which chunk is it? ("po-PO2024001-chunk-2")
///   - Payload: What does it say? (the original text + metadata)
///
/// The RagService then:
///   1. Takes the top results
///   2. Extracts the "chunkText" from each Payload
///   3. Builds a context string for Claude: "Here's what I found: ..."
///   4. Claude generates a grounded answer using that context
/// </summary>
public sealed class VectorSearchResult
{
    /// <summary>
    /// Cosine similarity score (0.0 to 1.0).
    /// Higher = more relevant.
    /// Typically: > 0.85 = highly relevant, 0.65–0.85 = somewhat relevant, &lt; 0.65 = noise.
    /// </summary>
    public required double Score { get; init; }

    /// <summary>
    /// The point's unique identifier (e.g., "style-S001-chunk-0").
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The metadata stored with this vector point.
    /// Always contains "chunkText" — the original text that was embedded.
    /// </summary>
    public required IReadOnlyDictionary<string, object> Payload { get; init; }
}
