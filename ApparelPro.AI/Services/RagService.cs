using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Models;
using ApparelPro.AI.Prompts;

namespace ApparelPro.AI.Services;

/// <summary>
/// Orchestrates the full RAG (Retrieval-Augmented Generation) query pipeline.
///
/// 🎓 THIS IS THE BRAIN OF RAG — WHERE EVERYTHING COMES TOGETHER
///
/// Think of RagService as a detective:
///   1. 🔍 INVESTIGATE: Take the user's question and convert it to a "search warrant" (embedding)
///   2. 📂 SEARCH: Raid the filing cabinet (Qdrant) for relevant documents (chunks)
///   3. 📋 COMPILE: Assemble the evidence into a case file (context block)
///   4. 🧠 ANALYSE: Hand the case file to the expert (Claude) for a conclusion
///   5. 📄 REPORT: Return the conclusion with citations (sources)
///
/// 🎓 DEPENDENCY INJECTION:
/// RagService depends on THREE other services (all injected via DI):
///
///   IEmbeddingService  → Converts text → vectors (OpenAI)
///   IVectorStoreService → Stores & searches vectors (Qdrant)
///   IAiService          → Generates answers from context (Claude/Anthropic)
///
/// Notice it uses INTERFACES, not concrete classes. This is DIP (SOLID's "D"):
///   - You can swap OpenAI embeddings for Cohere without changing this class
///   - You can swap Qdrant for Pinecone without changing this class
///   - You can swap Claude for GPT-4o without changing this class
///   - You can test this class with mock implementations
///
/// 🎓 LIFETIME: SINGLETON
/// RagService is registered as a singleton because:
///   - It's stateless — no per-request state is held between calls
///   - All its dependencies (IEmbeddingService, IVectorStoreService, IAiService) are also singletons
///   - There's no benefit to creating a new instance per request
///
/// Compare this to AiChatService which is SCOPED because it uses DbContext
/// (ApparelProDbContext is scoped — one per HTTP request).
/// </summary>
public sealed class RagService : IRagService
{
    // ── Dependencies ─────────────────────────────────────

    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStoreService;
    private readonly IAiService _aiService;
    private readonly RagSettings _ragSettings;
    private readonly ILogger<RagService> _logger;

    /// <summary>
    /// 🎓 CONSTRUCTOR INJECTION:
    /// All dependencies come through the constructor — never created with `new`.
    /// This makes the class testable and loosely coupled.
    ///
    /// Notice we inject IOptions&lt;RagSettings&gt; for configuration values:
    ///   - TopK: How many chunks to retrieve (default: 5)
    ///   - MinimumScore: Relevance threshold (default: 0.65)
    ///   - These come from appsettings.json → "AiSettings:Rag" section
    /// </summary>
    public RagService(
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStoreService,
        IAiService aiService,
        IOptions<RagSettings> ragSettings,
        ILogger<RagService> logger)
    {
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _aiService = aiService;
        _ragSettings = ragSettings.Value;
        _logger = logger;
    }

    // ── Public API ───────────────────────────────────────

    /// <inheritdoc />
    public async Task<RagQueryResponse> QueryAsync(
        string question,
        string? entityTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "RAG query started. Question: '{Question}', Filter: {Filter}",
            question, entityTypeFilter ?? "ALL");

        // ═══════════════════════════════════════════════
        //  STEP 1: EMBED THE QUESTION
        // ═══════════════════════════════════════════════
        //
        // 🎓 WHY EMBED THE QUESTION?
        // To search the vector store, we need the question in the SAME format
        // as the stored data — a 1536-dimensional vector.
        //
        // The embedding model understands MEANING, not just keywords:
        //   "Which styles use cotton?" → vector close to "Cotton T-Shirt, 100% cotton fabric"
        //   "What are our cheapest products?" → vector close to "Unit price: $3.50"
        //
        // This is why vector search is better than SQL LIKE '%cotton%':
        //   - "fabric materials" matches "textile consumption" (same meaning, different words)
        //   - "expensive" matches "high unit price" (semantic understanding)

        ReadOnlyMemory<float> questionVector;

        try
        {
            questionVector = await _embeddingService.EmbedAsync(question, cancellationToken);

            _logger.LogDebug(
                "Question embedded successfully. Vector dimensions: {Dimensions}",
                questionVector.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to embed question. Is the OpenAI API key configured?");
            throw;
        }

        // ═══════════════════════════════════════════════
        //  STEP 2: SEARCH THE VECTOR STORE
        // ═══════════════════════════════════════════════
        //
        // 🎓 WHAT HAPPENS HERE?
        // Qdrant receives our question vector and compares it against every stored vector
        // using cosine similarity. It returns the TopK most similar chunks.
        //
        // 🎓 THE PARAMETERS:
        // - queryVector: The embedded question (1536 floats)
        // - topK: Return at most this many results (default: 5)
        //   → More results = more context for Claude = better answers BUT more tokens = more cost
        //   → 5 is a good balance for most ERP queries
        //
        // - minimumScore: Only return results with similarity ≥ this value (default: 0.65)
        //   → Too low (0.3): Includes irrelevant noise → Claude gets confused
        //   → Too high (0.95): Only exact matches → misses relevant data
        //   → 0.65 is the sweet spot for our domain-specific chunks
        //
        // - entityTypeFilter: Optional filter (e.g., "Style")
        //   → Uses Qdrant's payload filtering (fast! happens before similarity computation)
        //   → null means "search everything"

        var searchResults = await _vectorStoreService.SearchAsync(
            questionVector,
            _ragSettings.TopK,
            _ragSettings.MinimumScore,
            entityTypeFilter,
            cancellationToken);

        _logger.LogInformation(
            "Vector search returned {Count} chunks above minimum score {MinScore}",
            searchResults.Count, _ragSettings.MinimumScore);

        // ═══════════════════════════════════════════════
        //  STEP 3: HANDLE "NO RESULTS" CASE
        // ═══════════════════════════════════════════════
        //
        // 🎓 WHY CHECK FOR EMPTY RESULTS?
        // If the vector store found nothing relevant, there's no point calling Claude.
        // We'd be sending: "Here's an empty context. Answer the question."
        // Claude would either hallucinate or say "I have no data" — both waste tokens.
        //
        // Instead, we return a helpful message immediately:
        //   - No API call to Claude = no cost
        //   - Instant response = better UX
        //   - Honest "no data" message = builds trust

        if (searchResults.Count == 0)
        {
            _logger.LogInformation("No relevant chunks found. Returning no-results response.");

            return new RagQueryResponse
            {
                Answer = RagPromptTemplates.BuildNoResultsMessage(question, entityTypeFilter),
                HasResults = false,
                Sources = [],
                RetrievedChunkCount = 0,
                Provider = "N/A",
                Model = "N/A",
                InputTokens = 0,
                OutputTokens = 0,
                TotalTokens = 0
            };
        }

        // ═══════════════════════════════════════════════
        //  STEP 4: BUILD CONTEXT FROM RETRIEVED CHUNKS
        // ═══════════════════════════════════════════════
        //
        // 🎓 WHAT IS "CONTEXT BUILDING"?
        // We take the raw search results and format them into a text block
        // that Claude can read naturally. Each chunk gets a numbered label
        // with its source entity info and relevance score.
        //
        // The result looks like:
        //   [1] (Style / S001, relevance: 0.92)
        //   Style S001 (Cotton T-Shirt) — Buyer: NEXT, Order: ORD-2024-001
        //   Quantity: 5,000 pcs, FOB: $4.50, Fabric: 100% cotton...
        //
        //   [2] (PurchaseOrder / PO-2024-001, relevance: 0.85)
        //   PO-2024-001 for buyer NEXT, Total qty: 5,000 pcs...
        //
        // 🎓 WHY NUMBERED?
        // Numbers help Claude reference specific sources in its answer:
        //   "According to source [1], Style S001 uses cotton..."
        // This makes the answer verifiable — the user can check chunk [1].

        var contextBlock = BuildContextBlock(searchResults);
        var sources = BuildSourceReferences(searchResults);

        // ═══════════════════════════════════════════════
        //  STEP 5: SEND TO CLAUDE FOR GENERATION
        // ═══════════════════════════════════════════════
        //
        // 🎓 THE MAGIC MOMENT — "Augmented Generation"
        // This is where "Retrieval-Augmented" meets "Generation":
        //   - System prompt: "You are an ERP assistant. Answer ONLY from the context."
        //   - User message: [retrieved context] + [original question]
        //   - Claude reads the context and generates a GROUNDED answer
        //
        // 🎓 WHY Temperature = 0.2?
        // We want Claude to be a FAITHFUL reporter of what's in the context,
        // not a creative writer. Low temperature = more deterministic:
        //   - Temperature 0.0: Completely deterministic (same answer every time)
        //   - Temperature 0.2: Almost deterministic, tiny variation for natural language
        //   - Temperature 0.7: Creative, diverse — BAD for factual RAG answers
        //
        // 🎓 WHY MaxTokens from settings?
        // DefaultMaxTokens (default: 2500) caps the response length.
        // RAG answers should be concise — 2-4 paragraphs. If the answer is getting
        // cut off, increase this value in appsettings.json.

        var userMessage = RagPromptTemplates.BuildRagUserMessage(contextBlock, question);

        var request = new AiCompletionRequest
        {
            SystemPrompt = RagPromptTemplates.RagSystemPrompt,
            UserMessage = userMessage,
            MaxTokens = 2500,       // Enough for a thorough RAG answer
            Temperature = 0.2       // Low — we want factual extraction, not creativity
        };

        var aiResponse = await _aiService.CompleteAsync(request, cancellationToken);

        _logger.LogInformation(
            "RAG query completed. Provider: {Provider}, Tokens: {Tokens}, Sources: {SourceCount}",
            aiResponse.Provider, aiResponse.TotalTokens, sources.Count);

        // ═══════════════════════════════════════════════
        //  STEP 6: ASSEMBLE THE RESPONSE
        // ═══════════════════════════════════════════════
        //
        // 🎓 EVERYTHING THE FRONTEND NEEDS:
        // - Answer: Claude's grounded response text
        // - Sources: Which entities contributed (for "show sources" UI)
        // - HasResults: true (we have data — if we didn't, we returned in Step 3)
        // - Token counts: For the admin dashboard / cost tracking
        // - Provider/Model: For debugging ("which model answered this?")

        return new RagQueryResponse
        {
            Answer = aiResponse.Content,
            HasResults = true,
            Sources = sources,
            RetrievedChunkCount = searchResults.Count,
            Provider = aiResponse.Provider,
            Model = aiResponse.Model,
            InputTokens = aiResponse.InputTokens,
            OutputTokens = aiResponse.OutputTokens,
            TotalTokens = aiResponse.TotalTokens
        };
    }

    // ── Private helpers ─────────────────────────────────

    /// <summary>
    /// Formats the search results into a numbered text block for Claude's context window.
    ///
    /// 🎓 CONTEXT FORMATTING STRATEGY:
    /// We want Claude to read the context like a human reads a reference document:
    ///   - Numbered entries for easy reference
    ///   - Entity type + key for identification
    ///   - Relevance score so Claude knows which sources to trust most
    ///   - The actual text content for the facts
    ///
    /// 🎓 WHY NOT JUST CONCATENATE?
    /// Raw concatenation loses structure:
    ///   "Cotton T-Shirt buyer NEXT 5000 pcs Polyester Jacket buyer H&amp;M 3000 pcs"
    ///   → Claude can't tell where one entity ends and another begins
    ///
    /// Formatted context preserves boundaries:
    ///   [1] (Style / S001, relevance: 0.92)
    ///   Cotton T-Shirt, Buyer: NEXT, 5,000 pcs
    ///
    ///   [2] (Style / S045, relevance: 0.85)
    ///   Polyester Jacket, Buyer: H&amp;M, 3,000 pcs
    ///   → Claude clearly sees two separate entities
    /// </summary>
    private static string BuildContextBlock(IReadOnlyList<VectorSearchResult> searchResults)
    {
        var chunks = new List<string>();

        for (var i = 0; i < searchResults.Count; i++)
        {
            var result = searchResults[i];

            // 🎓 EXTRACTING PAYLOAD:
            // Each VectorSearchResult carries a Payload dictionary.
            // We stored these fields during the sync job (EmbeddingSyncJob):
            //   - "entityType": "Style"
            //   - "entityKey": "S001"
            //   - "chunkText": The original text content
            //   - "chunkIndex": Which chunk number (0, 1, 2...)
            //   - "totalChunks": How many chunks this entity was split into
            var entityType = GetPayloadString(result.Payload, "entityType", "Unknown");
            var entityKey = GetPayloadString(result.Payload, "entityKey", "Unknown");
            var chunkText = GetPayloadString(result.Payload, "chunkText", "");

            chunks.Add(
                $"[{i + 1}] ({entityType} / {entityKey}, relevance: {result.Score:F2})\n{chunkText}");
        }

        return string.Join("\n\n", chunks);
    }

    /// <summary>
    /// Converts search results into source references for the API response.
    ///
    /// 🎓 SOURCE REFERENCES vs CONTEXT BLOCK:
    /// - Context block (BuildContextBlock): Full text, sent to Claude
    /// - Source references (this method): Metadata only, sent to the FRONTEND
    ///
    /// The frontend uses source references to:
    ///   1. Show "Sources" badges under the answer
    ///   2. Create clickable links to the original entity screens
    ///   3. Display relevance scores as visual indicators
    ///   4. Show chunk previews on hover
    /// </summary>
    private static List<RagSourceReference> BuildSourceReferences(
        IReadOnlyList<VectorSearchResult> searchResults)
    {
        return searchResults.Select(result =>
        {
            var chunkText = GetPayloadString(result.Payload, "chunkText", "");

            return new RagSourceReference
            {
                EntityType = GetPayloadString(result.Payload, "entityType", "Unknown"),
                EntityKey = GetPayloadString(result.Payload, "entityKey", "Unknown"),
                Score = result.Score,

                // 🎓 PREVIEW TRUNCATION:
                // The full chunk can be hundreds of characters.
                // We take the first 150 for a quick preview.
                // The "..." suffix signals there's more text in the original entity.
                ChunkPreview = chunkText.Length > 150
                    ? chunkText[..150] + "..."
                    : chunkText
            };
        }).ToList();
    }

    /// <summary>
    /// Safely extracts a string value from the search result payload dictionary.
    ///
    /// 🎓 WHY IS THIS NEEDED?
    /// The payload comes from Qdrant, which stores values as generic objects.
    /// The Qdrant .NET client may return them as different types:
    ///   - Strings come back as strings ✓
    ///   - Numbers might come back as long, double, or string
    ///   - Missing keys would throw KeyNotFoundException
    ///
    /// This helper method safely handles all cases:
    ///   - Key exists and is string → return the string
    ///   - Key exists but is another type → call ToString()
    ///   - Key doesn't exist → return the default value
    ///
    /// 🎓 DEFENSIVE PROGRAMMING:
    /// In a production system, never assume external data is in the expected format.
    /// Qdrant might change serialisation, or the data might be from an older schema.
    /// Always handle the unexpected gracefully.
    /// </summary>
    /// <param name="payload">The search result's payload dictionary.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">Value to return if the key is missing.</param>
    /// <returns>The string value, or the default.</returns>
    private static string GetPayloadString(
        IReadOnlyDictionary<string, object> payload,
        string key,
        string defaultValue)
    {
        if (payload.TryGetValue(key, out var value))
        {
            return value?.ToString() ?? defaultValue;
        }

        return defaultValue;
    }
}
