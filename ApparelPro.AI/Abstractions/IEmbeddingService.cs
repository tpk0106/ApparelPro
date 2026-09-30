namespace ApparelPro.AI.Abstractions;

/// <summary>
/// Abstraction for generating text embeddings (vectors).
///
/// 🎓 WHAT IS AN EMBEDDING SERVICE?
/// An embedding service takes text (like "Cotton fabric from Bangladesh supplier")
/// and converts it into a list of numbers (a "vector") — for example:
///   [0.023, -0.041, 0.089, ..., 0.017]   ← 1536 numbers
///
/// These numbers encode the MEANING of the text in a way that machines can compare.
/// Two pieces of text with similar meaning will have vectors that are close together
/// in "vector space" (measured by cosine similarity).
///
/// 🎓 WHY DO WE NEED THIS INTERFACE?
/// We follow the Dependency Inversion Principle (SOLID's "D"):
///   - The RAG pipeline depends on IEmbeddingService (abstraction)
///   - NOT on the concrete OpenAI embedding client (implementation)
///
/// This means we can:
///   1. Swap OpenAI for another provider (Cohere, local model) without changing RAG logic
///   2. Mock this interface in unit tests (no real API calls needed)
///   3. Add caching, retries, or logging as decorators around the same interface
///
/// 🎓 WHY OpenAI FOR EMBEDDINGS?
/// Anthropic (Claude) does NOT offer an embedding model — Claude is for text generation.
/// OpenAI's "text-embedding-3-small" is the industry standard for embeddings:
///   - Fast: ~100ms per request
///   - Cheap: $0.02 per million tokens
///   - Good quality: 1536 dimensions capture meaning accurately
/// The embedding model is INDEPENDENT of the generation model — we embed with OpenAI
/// and generate responses with Claude. No conflict, each does what it's best at.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Convert a single piece of text into its vector representation.
    ///
    /// 🎓 WHEN IS THIS USED?
    /// - When a user asks a question: we embed the question to search for similar vectors
    /// - When indexing a single entity update (e.g., one style was modified)
    ///
    /// Example:
    ///   Input:  "Which suppliers have late delivery records?"
    ///   Output: [0.023, -0.041, 0.089, ..., 0.017]  ← ReadOnlyMemory of 1536 floats
    ///
    /// 🎓 WHY ReadOnlyMemory<float>?
    /// - ReadOnlyMemory is more efficient than float[] — it avoids unnecessary copying
    /// - Qdrant's .NET client accepts ReadOnlyMemory<float> natively
    /// - It's the standard return type in the OpenAI .NET SDK for embeddings
    /// </summary>
    /// <param name="text">The text to convert into a vector embedding.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The embedding vector as a ReadOnlyMemory of floats.</returns>
    Task<ReadOnlyMemory<float>> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convert multiple pieces of text into their vector representations in a single API call.
    ///
    /// 🎓 WHEN IS THIS USED?
    /// - During batch indexing: when the sync job processes many entities at once
    /// - During initial data load: embedding all existing styles, POs, etc.
    ///
    /// 🎓 WHY A BATCH METHOD?
    /// Sending 100 texts in one API call is MUCH faster than 100 separate calls:
    ///   - 1 call with 100 texts: ~200ms total
    ///   - 100 calls with 1 text each: ~10,000ms total (100 × 100ms)
    /// The OpenAI embedding API accepts up to 2048 texts per request.
    ///
    /// Example:
    ///   Input:  ["Style S001: Cotton T-Shirt", "Style S002: Denim Jacket", ...]
    ///   Output: [[0.023, -0.041, ...], [0.056, 0.012, ...], ...]
    ///
    /// The output list is in the SAME ORDER as the input list — embedding[0]
    /// corresponds to texts[0], embedding[1] to texts[1], etc.
    /// </summary>
    /// <param name="texts">The list of texts to convert into vector embeddings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of embedding vectors, one per input text, in the same order.</returns>
    Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the dimensionality of the embeddings produced by this service.
    ///
    /// 🎓 WHY DO WE NEED THIS?
    /// When creating a Qdrant collection, we must tell it how many dimensions
    /// each vector will have. This must EXACTLY match what the embedding model produces.
    /// "text-embedding-3-small" produces 1536-dimension vectors.
    ///
    /// If Qdrant expects 1536 dimensions but we send 768, it will reject the vector.
    /// This property lets the vector store service ask the embedding service
    /// "how big are your vectors?" so it can configure the collection correctly.
    /// </summary>
    int Dimensions { get; }
}
