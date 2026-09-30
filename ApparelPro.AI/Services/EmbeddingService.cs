using System.ClientModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;

namespace ApparelPro.AI.Services;

/// <summary>
/// OpenAI-based embedding service that converts text into vector representations.
///
/// 🎓 HOW THIS SERVICE WORKS (Step by Step):
///
/// 1. Your .NET backend receives text to embed (e.g., "Cotton T-Shirt Style S001")
/// 2. This service sends that text to OpenAI's embedding API endpoint:
///    POST https://api.openai.com/v1/embeddings
///    Body: { "model": "text-embedding-3-small", "input": "Cotton T-Shirt Style S001" }
/// 3. OpenAI's model processes the text and returns a vector:
///    Response: { "data": [{ "embedding": [0.023, -0.041, 0.089, ...] }] }
/// 4. We extract the float array and return it to the caller
///
/// The caller (usually the EmbeddingSyncJob or RagService) then stores
/// this vector in Qdrant for later similarity search.
///
/// 🎓 COST AWARENESS:
/// - text-embedding-3-small: $0.02 per 1 million tokens
/// - Average style record chunk: ~200 tokens
/// - 1000 style chunks: 200,000 tokens = $0.004 (less than half a cent!)
/// - This is by far the cheapest AI API call you can make
///
/// 🎓 IMPORTANT ARCHITECTURE NOTE:
/// This service uses the SAME OpenAI NuGet package already in the project
/// (for the chat/completion provider), but accesses a DIFFERENT client:
///   - OpenAiProvider uses: openAiClient.GetChatClient(model)     → for text generation
///   - EmbeddingService uses: openAiClient.GetEmbeddingClient(model) → for embeddings
/// Same API key, same package, different capability.
/// </summary>
public sealed class EmbeddingService : IEmbeddingService
{
    // ── Private fields ───────────────────────────────────
    // Following the project's convention: private readonly, underscore-prefixed, camelCase

    private readonly EmbeddingClient? _embeddingClient;
    private readonly RagSettings _ragSettings;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly bool _isConfigured;

    /// <summary>
    /// 🎓 WHAT: Returns the number of dimensions each embedding vector has.
    /// WHY: The vector store (Qdrant) needs to know this when creating a collection.
    /// For "text-embedding-3-small", this is always 1536.
    /// </summary>
    public int Dimensions => _ragSettings.EmbeddingDimensions;

    // ── Constructor ──────────────────────────────────────

    /// <summary>
    /// 🎓 DEPENDENCY INJECTION PATTERN:
    /// The constructor doesn't create its own dependencies — it receives them
    /// from the DI container. This follows the same pattern as OpenAiProvider.
    ///
    /// Parameters explained:
    /// - IOptions&lt;AiSettings&gt;: We need the OpenAI API key (stored in AiSettings.OpenAI.ApiKey)
    /// - IOptions&lt;RagSettings&gt;: We need the embedding model name and dimensions
    /// - ILogger: For diagnostic logging (what's happening, how many tokens used)
    ///
    /// 🎓 WHY AiSettings AND RagSettings?
    /// - AiSettings holds the OpenAI API KEY (shared with the chat provider)
    /// - RagSettings holds RAG-specific config (which embedding model, dimensions)
    /// This separation keeps configuration clean — the API key isn't duplicated,
    /// and RAG settings don't pollute the general AI settings.
    /// </summary>
    public EmbeddingService(
        IOptions<AiSettings> aiSettings,
        IOptions<RagSettings> ragSettings,
        ILogger<EmbeddingService> logger)
    {
        _ragSettings = ragSettings.Value;
        _logger = logger;

        var openAiApiKey = aiSettings.Value.OpenAI.ApiKey;

        // ── Graceful degradation ─────────────────────
        // Same pattern as OpenAiProvider: if the API key is missing or placeholder,
        // we mark the service as "not configured" instead of crashing.
        // The app still starts up — it just can't generate embeddings until configured.
        if (string.IsNullOrWhiteSpace(openAiApiKey)
            || openAiApiKey.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "OpenAI API key is not configured. Embedding generation will be unavailable. " +
                "Set a valid key in AiSettings:OpenAI:ApiKey to enable RAG features.");
            _isConfigured = false;
            return;
        }

        // ── Create the embedding client ──────────────
        // 🎓 HOW THE OpenAI .NET SDK WORKS:
        //   1. Create a top-level OpenAIClient with the API key
        //   2. Call .GetEmbeddingClient(modelName) to get a model-specific client
        //   3. Use that client's methods to generate embeddings
        //
        // The EmbeddingClient is thread-safe and reusable — we create it once
        // in the constructor and use it for the lifetime of the application.
        var openAiClient = new OpenAIClient(new ApiKeyCredential(openAiApiKey));
        _embeddingClient = openAiClient.GetEmbeddingClient(_ragSettings.EmbeddingModel);
        _isConfigured = true;

        _logger.LogInformation(
            "EmbeddingService initialised with model '{Model}', dimensions: {Dimensions}",
            _ragSettings.EmbeddingModel,
            _ragSettings.EmbeddingDimensions);
    }

    // ── Single text embedding ────────────────────────────

    /// <summary>
    /// 🎓 THE CORE OPERATION: Text → Vector
    ///
    /// This is the fundamental building block of RAG. Every other operation
    /// (batch embedding, similarity search, etc.) builds on this concept.
    ///
    /// What happens inside:
    ///   1. We validate the input (not null, not empty, service is configured)
    ///   2. We call OpenAI's embedding API with the text
    ///   3. OpenAI returns a vector (1536 floats)
    ///   4. We extract the vector and return it
    ///
    /// The returned vector can then be:
    ///   - Stored in Qdrant (during indexing)
    ///   - Used to search Qdrant (during query time)
    /// </summary>
    public async Task<ReadOnlyMemory<float>> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        // ── Guard clauses ────────────────────────────
        // 🎓 DEFENSIVE PROGRAMMING: Always validate inputs before making API calls.
        // An empty string would waste an API call and return a meaningless vector.
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Cannot embed empty or whitespace text. " +
                "The text must contain meaningful content to generate a useful embedding.",
                nameof(text));
        }

        try
        {
            _logger.LogDebug(
                "Generating embedding for text ({Length} chars) using model '{Model}'",
                text.Length,
                _ragSettings.EmbeddingModel);

            // ── Call the OpenAI Embedding API ────────
            // 🎓 WHAT HAPPENS OVER THE WIRE:
            //   POST https://api.openai.com/v1/embeddings
            //   Headers: Authorization: Bearer sk-...
            //   Body: {
            //     "model": "text-embedding-3-small",
            //     "input": "Cotton T-Shirt Style S001 for Buyer NEXT...",
            //     "dimensions": 1536
            //   }
            //
            // GenerateEmbeddingAsync is the OpenAI .NET SDK's method for single-text embedding.
            // It returns an OpenAIEmbedding object containing the vector and usage stats.
            OpenAIEmbedding embedding = await _embeddingClient!.GenerateEmbeddingAsync(
                text,
                new EmbeddingGenerationOptions
                {
                    Dimensions = _ragSettings.EmbeddingDimensions
                },
                cancellationToken);

            // ── Extract the vector ───────────────────
            // 🎓 WHAT IS .ToFloats()?
            // The SDK returns the embedding in a compact binary format.
            // .ToFloats() converts it to a ReadOnlyMemory<float> — the format
            // that Qdrant's .NET client expects for storing/searching vectors.
            ReadOnlyMemory<float> vector = embedding.ToFloats();

            _logger.LogDebug(
                "Embedding generated successfully. Vector dimensions: {Dimensions}",
                vector.Length);

            return vector;
        }
        catch (ClientResultException ex)
        {
            // 🎓 ClientResultException is the OpenAI SDK's exception type for API errors.
            // Common causes:
            //   - 401: Invalid API key
            //   - 429: Rate limit exceeded (too many requests per minute)
            //   - 500: OpenAI service error
            _logger.LogError(ex,
                "OpenAI embedding API error. Status: {Status}. " +
                "Check your API key and rate limits.",
                ex.Status);
            throw;
        }
    }

    // ── Batch text embedding ─────────────────────────────

    /// <summary>
    /// 🎓 BATCH EMBEDDING: Multiple texts → Multiple vectors in ONE API call
    ///
    /// This is the workhorse method used during data synchronisation.
    /// When the EmbeddingSyncJob finds 50 styles that need re-indexing,
    /// it calls this method once instead of calling EmbedAsync 50 times.
    ///
    /// 🎓 PERFORMANCE COMPARISON:
    ///   50 separate calls: 50 × ~100ms = ~5 seconds + 50 HTTP round trips
    ///   1 batch call:      1 × ~200ms  = ~200ms    + 1 HTTP round trip
    ///   That's a 25× speedup!
    ///
    /// 🎓 API LIMITS:
    /// OpenAI allows up to 2048 texts per embedding request.
    /// If you need more, you'd batch them into groups of 2048.
    /// For ApparelPro, even 1000 style chunks is well within limits.
    /// </summary>
    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        if (texts is null || texts.Count == 0)
        {
            throw new ArgumentException(
                "Cannot embed an empty list of texts. Provide at least one text to embed.",
                nameof(texts));
        }

        // ── Filter out empty strings ─────────────────
        // 🎓 WHY THIS CHECK?
        // If someone accidentally passes ["Style S001", "", "Style S003"],
        // the empty string would waste API tokens and produce a meaningless vector.
        // We validate upfront rather than sending garbage to the API.
        var hasEmpty = texts.Any(t => string.IsNullOrWhiteSpace(t));
        if (hasEmpty)
        {
            throw new ArgumentException(
                "All texts in the batch must be non-empty. " +
                "Remove or filter out any null/empty/whitespace entries before calling EmbedBatchAsync.",
                nameof(texts));
        }

        try
        {
            _logger.LogInformation(
                "Generating batch embeddings for {Count} texts using model '{Model}'",
                texts.Count,
                _ragSettings.EmbeddingModel);

            // ── Call the OpenAI Batch Embedding API ──
            // 🎓 WHAT HAPPENS OVER THE WIRE:
            //   POST https://api.openai.com/v1/embeddings
            //   Body: {
            //     "model": "text-embedding-3-small",
            //     "input": ["text1", "text2", "text3", ...],
            //     "dimensions": 1536
            //   }
            //
            // GenerateEmbeddingsAsync (plural!) accepts a list of strings.
            // It returns an EmbeddingCollection containing one embedding per input text.
            OpenAIEmbeddingCollection embeddings = await _embeddingClient!.GenerateEmbeddingsAsync(
                texts,
                new EmbeddingGenerationOptions
                {
                    Dimensions = _ragSettings.EmbeddingDimensions
                },
                cancellationToken);

            // ── Convert to our return type ───────────
            // 🎓 WHY THE .OrderBy(e => e.Index)?
            // The OpenAI API GUARANTEES the response order matches the input order,
            // but the SDK's collection type doesn't enforce this in the type system.
            // Sorting by Index is a safety measure — defensive programming.
            // In practice, they always come back in order, but "always" is a
            // dangerous word in production code.
            var vectors = embeddings
                .OrderBy(e => e.Index)
                .Select(e => e.ToFloats())
                .ToList();

            _logger.LogInformation(
                "Batch embedding complete. Generated {Count} vectors of {Dimensions} dimensions each",
                vectors.Count,
                vectors.FirstOrDefault().Length);

            return vectors;
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex,
                "OpenAI batch embedding API error. Status: {Status}. " +
                "Attempted to embed {Count} texts.",
                ex.Status,
                texts.Count);
            throw;
        }
    }

    // ── Private helpers ──────────────────────────────────

    /// <summary>
    /// 🎓 GUARD METHOD: Ensures the service was properly configured at startup.
    /// Following the same pattern as OpenAiProvider — if the API key was missing
    /// during construction, we throw a clear error explaining what to do.
    ///
    /// This is called at the START of every public method before doing any work.
    /// It's a "fail fast" pattern — better to throw immediately with a helpful
    /// message than to get a cryptic NullReferenceException later.
    /// </summary>
    private void EnsureConfigured()
    {
        if (!_isConfigured || _embeddingClient is null)
        {
            throw new InvalidOperationException(
                "EmbeddingService is not configured. The OpenAI API key is missing or invalid. " +
                "Set a valid key in AiSettings:OpenAI:ApiKey (in appsettings.json, User Secrets, " +
                "or Coolify environment variables) to enable RAG embedding features.");
        }
    }
}
