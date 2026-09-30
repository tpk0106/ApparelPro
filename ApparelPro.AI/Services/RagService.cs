using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Models;
using ApparelPro.AI.Prompts;
using apparelPro.BusinessLogic.Services;

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
/// 🎓 WHAT CHANGED — REPORT INTENT DETECTION:
/// RagService now also:
///   6. 📊 DETECT: Check if Claude identified a report request in its response
///   7. 📥 EXTRACT: Parse the |||REPORT_INTENT||| block into a typed model
///
/// 🎓 DEPENDENCY INJECTION:
/// RagService depends on THREE other services (all injected via DI):
///
///   IEmbeddingService  → Converts text → vectors (OpenAI)
///   IVectorStoreService → Stores &amp; searches vectors (Qdrant)
///   IAiService          → Generates answers from context (Claude/Anthropic)
///
/// 🆕 Plus IServiceScopeFactory for resolving IReportRegistryService.
///
/// 🎓 WHY IServiceScopeFactory INSTEAD OF DIRECT INJECTION?
/// RagService is a SINGLETON (stateless, long-lived).
/// IReportRegistryService is TRANSIENT and uses ApparelProDbContext which is SCOPED.
///
/// In .NET DI, a singleton CANNOT directly consume a scoped service — this is called
/// a "captive dependency" and causes the DbContext to be held open for the app's lifetime,
/// leading to memory leaks and stale data.
///
/// The solution: inject IServiceScopeFactory and create a short-lived scope
/// each time we need the report registry. The scope creates a fresh DbContext,
/// we read the data, and the scope disposes everything cleanly.
///
/// 🎓 LIFETIME: SINGLETON
/// RagService is registered as a singleton because:
///   - It's stateless — no per-request state is held between calls
///   - All its core dependencies (IEmbeddingService, IVectorStoreService, IAiService) are also singletons
///   - IServiceScopeFactory is safe to hold in a singleton (it's designed for this pattern)
///
/// Compare this to AiChatService which is SCOPED because it uses DbContext directly.
/// </summary>
public sealed class RagService : IRagService
{
    // ── Dependencies ─────────────────────────────────

    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStoreService;
    private readonly IAiService _aiService;
    private readonly RagSettings _ragSettings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RagService> _logger;

    // 🎓 INTENT MARKERS:
    // These are the delimiters Claude uses to mark intent blocks in its response.
    // We parse the response text to find content between these markers.
    // Triple pipes + uppercase = impossible to confuse with regular text.
    private const string IntentStartMarker = "|||REPORT_INTENT|||";
    private const string IntentEndMarker = "|||END_REPORT_INTENT|||";

    /// <summary>
    /// 🎓 CONSTRUCTOR INJECTION:
    /// All dependencies come through the constructor — never created with `new`.
    /// This makes the class testable and loosely coupled.
    ///
    /// 🆕 IServiceScopeFactory added for safe access to scoped IReportRegistryService.
    /// </summary>
    public RagService(
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStoreService,
        IAiService aiService,
        IOptions<RagSettings> ragSettings,
        IServiceScopeFactory scopeFactory,
        ILogger<RagService> logger)
    {
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _aiService = aiService;
        _ragSettings = ragSettings.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // ── Public API ───────────────────────────────────

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
        //  STEP 0: LOAD REPORT CATALOGUE
        // ═══════════════════════════════════════════════
        //
        // 🎓 WHY LOAD EVERY TIME (NOT CACHED)?
        // The report registry is tiny (a handful of rows) and rarely changes.
        // Loading it per-query is cheap (~1ms DB call) and guarantees we always
        // see the latest data without cache invalidation complexity.
        //
        // If the registry grows to hundreds of entries (unlikely), consider
        // adding IMemoryCache with a short TTL (5 minutes).
        //
        // 🎓 WHY TRY/CATCH?
        // If the report registry fails to load (DB down, migration not run yet),
        // we DON'T want to fail the entire RAG query. We fall back to the base
        // prompt without report detection — the query still works for questions.

        string? reportCatalogue = null;

        try
        {
            reportCatalogue = await LoadReportCatalogueAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to load report catalogue. RAG will proceed without report intent detection.");
        }

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
        // We return a helpful message immediately — no cost, instant response.

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
                TotalTokens = 0,
                DetectedReportIntent = null
            };
        }

        // ═══════════════════════════════════════════════
        //  STEP 4: BUILD CONTEXT FROM RETRIEVED CHUNKS
        // ═══════════════════════════════════════════════

        var contextBlock = BuildContextBlock(searchResults);
        var sources = BuildSourceReferences(searchResults);

        // ═══════════════════════════════════════════════
        //  STEP 5: SEND TO CLAUDE FOR GENERATION
        // ═══════════════════════════════════════════════
        //
        // 🎓 WHAT CHANGED:
        // We now use BuildFullSystemPrompt() which includes the report catalogue.
        // This means Claude can detect report intents from natural language.

        var userMessage = RagPromptTemplates.BuildRagUserMessage(contextBlock, question);

        // 🎓 Build the system prompt WITH the report catalogue
        var systemPrompt = RagPromptTemplates.BuildFullSystemPrompt(reportCatalogue);

        var request = new AiCompletionRequest
        {
            SystemPrompt = systemPrompt,
            UserMessage = userMessage,
            MaxTokens = 2500,       // Enough for a thorough RAG answer + intent block
            Temperature = 0.2       // Low — we want factual extraction, not creativity
        };

        var aiResponse = await _aiService.CompleteAsync(request, cancellationToken);

        _logger.LogInformation(
            "RAG query completed. Provider: {Provider}, Tokens: {Tokens}, Sources: {SourceCount}",
            aiResponse.Provider, aiResponse.TotalTokens, sources.Count);

        // ═══════════════════════════════════════════════
        //  STEP 6: PARSE REPORT INTENT (IF PRESENT)
        // ═══════════════════════════════════════════════
        //
        // 🎓 NEW STEP — INTENT EXTRACTION:
        // Check if Claude's response contains the |||REPORT_INTENT||| marker.
        // If yes, parse the JSON between the markers into a ReportIntentDetection model.
        // If no, DetectedReportIntent stays null (it's a regular RAG answer).
        //
        // 🎓 WHY PARSE SERVER-SIDE?
        // - The frontend shouldn't need to know about the marker format
        // - Server-side parsing gives us a clean typed model
        // - We can validate the intent (lookup EndpointTemplate) before sending to frontend
        // - We strip the marker block from the answer text so the user never sees it

        var (cleanAnswer, detectedIntent) = await ParseReportIntentAsync(aiResponse.Content);

        // ═══════════════════════════════════════════════
        //  STEP 7: ASSEMBLE THE RESPONSE
        // ═══════════════════════════════════════════════

        if (detectedIntent != null)
        {
            _logger.LogInformation(
                "Report intent detected: {ReportCode} with confidence {Confidence:F2}. Params: {ParamCount}",
                detectedIntent.ReportCode, detectedIntent.Confidence, detectedIntent.Parameters.Count);
        }

        return new RagQueryResponse
        {
            Answer = cleanAnswer,           // 🎓 Text without the |||REPORT_INTENT||| block
            HasResults = true,
            Sources = sources,
            RetrievedChunkCount = searchResults.Count,
            Provider = aiResponse.Provider,
            Model = aiResponse.Model,
            InputTokens = aiResponse.InputTokens,
            OutputTokens = aiResponse.OutputTokens,
            TotalTokens = aiResponse.TotalTokens,
            DetectedReportIntent = detectedIntent   // 🆕 null if no report request detected
        };
    }

    // ── Private helpers ─────────────────────────────────

    /// <summary>
    /// 🆕 Loads the report catalogue from the database via a scoped IReportRegistryService.
    ///
    /// 🎓 THE SCOPE PATTERN:
    /// 1. Create a new DI scope (like a mini HTTP request)
    /// 2. Resolve IReportRegistryService from that scope (gets a fresh DbContext)
    /// 3. Call GetActiveReportsAsync() to read the registry
    /// 4. Build the prompt catalogue string
    /// 5. Dispose the scope (DbContext is disposed, connection returned to pool)
    ///
    /// All of this happens in ~1ms for a small table like ReportRegistries.
    /// </summary>
    private async Task<string?> LoadReportCatalogueAsync()
    {
        // 🎓 SCOPED RESOLUTION:
        // using var scope = ... creates a short-lived scope.
        // When the scope disposes, everything resolved from it (including DbContext) is disposed.
        using var scope = _scopeFactory.CreateScope();

        var reportRegistryService = scope.ServiceProvider
            .GetRequiredService<IReportRegistryService>();

        var activeReports = await reportRegistryService.GetActiveReportsAsync();
        var reportsList = activeReports.ToList();

        if (reportsList.Count == 0)
        {
            _logger.LogDebug("No active reports found in registry. Skipping report catalogue.");
            return null;
        }

        _logger.LogDebug("Loaded {Count} active reports for prompt catalogue.", reportsList.Count);

        return RagPromptTemplates.BuildReportCatalogueBlock(reportsList);
    }

    /// <summary>
    /// 🆕 Parses Claude's response to extract a report intent block (if present).
    ///
    /// 🎓 PARSING STRATEGY:
    /// 1. Search for |||REPORT_INTENT||| marker in the response text
    /// 2. If not found → return (original text, null) — no intent detected
    /// 3. If found → extract the JSON between start and end markers
    /// 4. Parse the JSON into a ReportIntentDetection model
    /// 5. Look up the EndpointTemplate from the registry (Claude doesn't output this)
    /// 6. Strip the marker block from the text response
    /// 7. Return (clean text, intent model)
    ///
    /// 🎓 WHY LOOK UP EndpointTemplate SEPARATELY?
    /// We don't include EndpointTemplate in Claude's prompt because:
    ///   - It's an internal API detail — Claude doesn't need it to detect intent
    ///   - Less data in the prompt = fewer tokens = lower cost
    ///   - We validate the report code server-side anyway
    /// </summary>
    private async Task<(string cleanAnswer, ReportIntentDetection? intent)> ParseReportIntentAsync(
        string rawResponse)
    {
        // ── Check for the intent marker ─────────────────
        var startIndex = rawResponse.IndexOf(IntentStartMarker, StringComparison.Ordinal);

        if (startIndex < 0)
        {
            // No intent detected — return the original response as-is
            return (rawResponse, null);
        }

        var endIndex = rawResponse.IndexOf(IntentEndMarker, startIndex, StringComparison.Ordinal);

        if (endIndex < 0)
        {
            // Start marker found but no end marker — malformed output from Claude.
            // Log it and return the raw response (don't crash the query).
            _logger.LogWarning(
                "Found {StartMarker} but no {EndMarker} in Claude's response. " +
                "Treating as no intent detected.",
                IntentStartMarker, IntentEndMarker);

            return (rawResponse, null);
        }

        // ── Extract the JSON between markers ────────────
        var jsonStart = startIndex + IntentStartMarker.Length;
        var intentJson = rawResponse[jsonStart..endIndex].Trim();

        // ── Clean the answer text (remove the marker block) ──
        // 🎓 We strip everything from |||REPORT_INTENT||| to |||END_REPORT_INTENT|||
        // so the user sees a clean text response without the JSON block.
        var cleanAnswer = rawResponse[..startIndex].TrimEnd();

        // Also remove any trailing whitespace/newlines after the end marker
        var afterEndMarker = endIndex + IntentEndMarker.Length;
        if (afterEndMarker < rawResponse.Length)
        {
            var remainder = rawResponse[afterEndMarker..].TrimStart();
            if (!string.IsNullOrEmpty(remainder))
            {
                cleanAnswer += "\n\n" + remainder;
            }
        }

        // ── Parse the JSON ──────────────────────────────
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var intentData = JsonSerializer.Deserialize<IntentJsonPayload>(intentJson, options);

            if (intentData == null || string.IsNullOrWhiteSpace(intentData.ReportCode))
            {
                _logger.LogWarning("Parsed intent JSON but reportCode was empty. Ignoring.");
                return (cleanAnswer, null);
            }

            // ── Look up the EndpointTemplate from the registry ──
            // 🎓 Claude outputs the reportCode and parameters,
            // but we look up the EndpointTemplate server-side for security.
            // This prevents prompt injection from pointing to arbitrary endpoints.
            string endpointTemplate;
            string displayName;

            using (var scope = _scopeFactory.CreateScope())
            {
                var registryService = scope.ServiceProvider
                    .GetRequiredService<IReportRegistryService>();

                var report = await registryService.GetReportByCodeAsync(intentData.ReportCode);

                if (report == null)
                {
                    _logger.LogWarning(
                        "Claude detected report code '{ReportCode}' but it doesn't exist in the registry. " +
                        "Ignoring intent.",
                        intentData.ReportCode);

                    return (cleanAnswer, null);
                }

                endpointTemplate = report.EndpointTemplate;
                displayName = report.DisplayName;
            }

            var detectedIntent = new ReportIntentDetection
            {
                ReportCode = intentData.ReportCode,
                DisplayName = displayName,
                EndpointTemplate = endpointTemplate,
                Parameters = intentData.Parameters ?? new Dictionary<string, string>(),
                Confidence = intentData.Confidence
            };

            return (cleanAnswer, detectedIntent);
        }
        catch (JsonException ex)
        {
            // 🎓 Claude sometimes produces slightly malformed JSON.
            // Rather than crashing the entire query, we log the error
            // and return the answer without intent detection.
            _logger.LogWarning(
                ex,
                "Failed to parse report intent JSON from Claude's response. Raw JSON: {Json}",
                intentJson.Length > 500 ? intentJson[..500] + "..." : intentJson);

            return (cleanAnswer, null);
        }
    }

    /// <summary>
    /// Formats the search results into a numbered text block for Claude's context window.
    ///
    /// 🎓 CONTEXT FORMATTING STRATEGY:
    /// We want Claude to read the context like a human reads a reference document:
    ///   - Numbered entries for easy reference
    ///   - Entity type + key for identification
    ///   - Relevance score so Claude knows which sources to trust most
    ///   - The actual text content for the facts
    /// </summary>
    private static string BuildContextBlock(IReadOnlyList<VectorSearchResult> searchResults)
    {
        var chunks = new List<string>();

        for (var i = 0; i < searchResults.Count; i++)
        {
            var result = searchResults[i];

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
                ChunkPreview = chunkText.Length > 150
                    ? chunkText[..150] + "..."
                    : chunkText
            };
        }).ToList();
    }

    /// <summary>
    /// Safely extracts a string value from the search result payload dictionary.
    /// </summary>
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

    // ── Internal deserialization model ───────────────

    /// <summary>
    /// 🎓 INTERNAL JSON SHAPE:
    /// This is the shape of the JSON that Claude outputs between the
    /// |||REPORT_INTENT||| markers. It's internal to RagService — the
    /// public API uses ReportIntentDetection which includes EndpointTemplate
    /// (looked up server-side, not from Claude's output).
    ///
    /// Keeping this private ensures Claude's raw output format is an
    /// implementation detail, not a public contract.
    /// </summary>
    private sealed class IntentJsonPayload
    {
        public string ReportCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public Dictionary<string, string>? Parameters { get; set; }
    }
}
