namespace ApparelPro.AI.Configuration;

/// <summary>
/// Configuration for the RAG (Retrieval-Augmented Generation) pipeline.
/// Bound from appsettings.json section "AiSettings:Rag".
///
/// 🎓 WHAT IS THIS?
/// This class holds all the "knobs" for the RAG system. Instead of
/// hardcoding values like the Qdrant URL or chunk size, we put them
/// in configuration so you can change them in appsettings.json or
/// Coolify environment variables without recompiling.
///
/// 🎓 WHY THESE SETTINGS?
/// - QdrantUrl: Where the vector database lives (your Coolify Docker container)
/// - CollectionName: Like a "table" in Qdrant — we store all ApparelPro vectors in one collection
/// - EmbeddingModel: Which AI model converts text → vectors
/// - ChunkSize: How many tokens per text chunk (too big = wastes tokens, too small = loses context)
/// - ChunkOverlap: Overlap between chunks so we don't cut sentences in half
/// - TopK: How many similar results to retrieve (more = better context but higher token cost)
/// - MinimumScore: Threshold to filter out irrelevant results (0.0 to 1.0)
/// </summary>
public sealed class RagSettings
{
    /// <summary>
    /// 🎓 WHAT: The URL where Qdrant is running.
    /// WHY: Your .NET backend needs to know where to send vector storage/search requests.
    /// In Coolify, Qdrant runs as a sibling container, so this is typically "http://qdrant:6333"
    /// (Docker DNS) or "http://localhost:6333" for local development.
    /// </summary>
    public string QdrantUrl { get; set; } = "http://localhost:6333";

    /// <summary>
    /// 🎓 WHAT: Optional API key for Qdrant authentication.
    /// WHY: In production, you should secure Qdrant with an API key so only your backend
    /// can access it. Without this, anyone who can reach port 6333 can read your vectors.
    /// Set via environment variable: AiSettings__Rag__QdrantApiKey
    /// </summary>
    public string? QdrantApiKey { get; set; }

    /// <summary>
    /// 🎓 WHAT: The name of the Qdrant "collection" (like a database table).
    /// WHY: All ApparelPro entity vectors go into one collection. Qdrant lets you
    /// filter within a collection by metadata (entity type, buyer code, etc.),
    /// so one collection is simpler than creating separate ones per entity type.
    /// </summary>
    public string CollectionName { get; set; } = "apparelpro-entities";

    /// <summary>
    /// 🎓 WHAT: The embedding model that converts text into vectors.
    /// WHY: "text-embedding-3-small" is OpenAI's latest small embedding model.
    /// It produces 1536-dimension vectors and costs only $0.02 per million tokens.
    /// We use OpenAI for embeddings because:
    ///   1. Anthropic doesn't offer an embedding model (Claude is for generation only)
    ///   2. OpenAI's embedding API is fast, cheap, and well-supported
    ///   3. The embedding model is independent of the generation model —
    ///      we can embed with OpenAI and generate with Claude, no conflict.
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// 🎓 WHAT: The dimensionality (length) of the embedding vectors.
    /// WHY: Must match what the embedding model produces.
    /// "text-embedding-3-small" outputs 1536 dimensions by default.
    /// Each dimension captures one aspect of the text's meaning.
    /// Think of it like GPS coordinates, but instead of 2 dimensions (lat/long),
    /// we use 1536 dimensions to pinpoint meaning in a high-dimensional space.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1536;

    /// <summary>
    /// 🎓 WHAT: Maximum number of tokens per text chunk.
    /// WHY: We can't embed an entire style record as one giant blob — embedding models
    /// have token limits and long texts dilute the meaning. 500 tokens (~375 words)
    /// is a sweet spot: big enough to capture a complete thought, small enough
    /// to keep the embedding focused.
    ///
    /// Example: A style with 20 material consumption rows gets split into:
    ///   Chunk 1: Style header + buyer info + first 8 materials
    ///   Chunk 2: Style header (repeated for context) + materials 7-15
    ///   Chunk 3: Style header (repeated for context) + materials 14-20
    /// </summary>
    public int ChunkSize { get; set; } = 500;

    /// <summary>
    /// 🎓 WHAT: How many tokens of overlap between consecutive chunks.
    /// WHY: If we split text at exactly 500 tokens, we might cut a sentence in half:
    ///   "The cotton fabric from Supplier 105 has a lead time of..." | "...14 days and costs $2.50/metre"
    /// With 50 tokens of overlap, the end of chunk N is repeated at the start of chunk N+1,
    /// so the full sentence appears in at least one chunk.
    /// </summary>
    public int ChunkOverlap { get; set; } = 50;

    /// <summary>
    /// 🎓 WHAT: How many similar results to retrieve from the vector store.
    /// WHY: When a user asks "which suppliers delivered late?", we search Qdrant
    /// and get the top K most relevant chunks. More results = more context for Claude,
    /// but also more tokens = higher cost and slower response.
    /// 5 is a good starting point — we can tune up or down based on answer quality.
    /// </summary>
    public int TopK { get; set; } = 5;

    /// <summary>
    /// 🎓 WHAT: Minimum cosine similarity score (0.0 to 1.0) to include a result.
    /// WHY: Even the "most similar" vector might be completely irrelevant if the
    /// user asks about something we have no data on. A score below 0.65 usually
    /// means "this chunk has little to do with the query" — better to return
    /// fewer, high-quality results than pad the context with noise.
    ///
    /// 🎓 WHAT IS COSINE SIMILARITY?
    /// It measures the angle between two vectors. If they point in the same direction
    /// (meaning is similar), the score is close to 1.0. If they're perpendicular
    /// (unrelated), it's close to 0.0. If opposite (antonyms), it's negative.
    /// </summary>
    public double MinimumScore { get; set; } = 0.65;

    /// <summary>
    /// 🎓 WHAT: How often (in minutes) the background sync job checks for updated entities.
    /// WHY: When someone updates a Style or adds a new PO in ApparelPro, the vector store
    /// needs to be updated too. This interval controls how quickly RAG "learns" about
    /// new data. 15 minutes is a good balance — frequent enough to be useful,
    /// infrequent enough to not overload the embedding API.
    /// </summary>
    public int SyncIntervalMinutes { get; set; } = 15;
}
