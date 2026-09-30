namespace ApparelPro.AI.Models;

/// <summary>
/// Response from a RAG query pipeline execution.
///
/// 🎓 WHAT'S IN THIS RESPONSE?
/// When the frontend calls POST /api/rag/query, it gets this back.
/// It contains everything the UI needs to display the answer:
///
///   {
///     "answer": "Based on the data, two styles use cotton fabric...",
///     "hasResults": true,
///     "sources": [
///       { "entityType": "Style", "entityKey": "S001", "score": 0.92, "chunkPreview": "Cotton T-Shirt..." },
///       { "entityType": "Style", "entityKey": "S045", "score": 0.85, "chunkPreview": "Cotton Polo..." }
///     ],
///     "provider": "Anthropic",
///     "model": "claude-sonnet-4-20250514",
///     "totalTokens": 1250,
///     "retrievedChunkCount": 5
///   }
///
/// 🎓 WHY INCLUDE SOURCES?
/// Transparency is crucial for enterprise AI:
///   - Users can VERIFY the answer by clicking through to the source entities
///   - If the answer seems wrong, the sources show which data Claude was looking at
///   - Auditors can trace HOW the AI reached its conclusion
///
/// This is the difference between RAG and hallucination:
///   Hallucination: "Your cotton consumption is 2.5 yards" (where did that come from??)
///   RAG with sources: "Based on Style S001 (score: 0.92), cotton consumption is 2.5 yards"
///
/// 🎓 WHAT CHANGED — REPORT INTENT DETECTION:
/// When the user asks something like "Can I get the Trim Sheet for style ANCHORAGE?",
/// Claude now detects this as a REPORT REQUEST and includes intent data in its response.
/// The frontend checks DetectedReportIntent — if it's not null, the UI can auto-trigger
/// the PDF generation flow instead of just showing a text answer.
/// </summary>
public sealed class RagQueryResponse
{
    /// <summary>
    /// Claude's generated answer, grounded in the retrieved context.
    /// If no relevant data was found, this contains a "no data found" message.
    ///
    /// 🎓 WHEN A REPORT INTENT IS DETECTED:
    /// The Answer will STILL contain a helpful text response, something like:
    ///   "I found Style ANCHORAGE under buyer NEXT. I can generate the Trim Sheet report for you."
    /// The frontend can show this text AND trigger the PDF generation using DetectedReportIntent.
    /// </summary>
    public required string Answer { get; init; }

    /// <summary>
    /// Whether the vector store found any chunks above the minimum relevance score.
    /// When false, the Answer is a "no data found" message — NOT a hallucinated answer.
    ///
    /// 🎓 WHY IS THIS IMPORTANT?
    /// The frontend can use this to show different UI:
    ///   hasResults = true  → Show answer with source links
    ///   hasResults = false → Show "No matching data found" with a suggestion to rephrase
    /// </summary>
    public bool HasResults { get; init; }

    /// <summary>
    /// The source chunks that were used to generate the answer.
    /// Ordered by relevance score (highest first).
    ///
    /// 🎓 WHY A LIST?
    /// Multiple chunks might contribute to the answer:
    ///   Question: "Compare cotton and polyester styles"
    ///   Sources: [cotton style chunk, polyester style chunk, ...]
    /// The UI can render these as clickable "evidence cards".
    /// </summary>
    public IReadOnlyList<RagSourceReference> Sources { get; init; } = [];

    /// <summary>
    /// How many chunks were retrieved from the vector store.
    /// This is the count BEFORE any deduplication — it matches TopK from settings.
    /// Useful for diagnostics: if retrievedChunkCount = 0, the query didn't match anything.
    /// </summary>
    public int RetrievedChunkCount { get; init; }

    /// <summary>
    /// Which AI provider generated the answer (e.g., "Anthropic").
    /// </summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// Which model was used (e.g., "claude-sonnet-4-20250514").
    /// </summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Input tokens consumed (the context + question sent to Claude).
    /// Useful for cost tracking — this is where most of the cost is in RAG.
    /// </summary>
    public int InputTokens { get; init; }

    /// <summary>
    /// Output tokens generated (the answer Claude produced).
    /// </summary>
    public int OutputTokens { get; init; }

    /// <summary>
    /// Total tokens consumed (input + output) for cost tracking.
    /// </summary>
    public int TotalTokens { get; init; }

    /// <summary>
    /// 🆕 When Claude detects the user is requesting a REPORT (not just asking a question),
    /// this contains the detected intent: which report, and the extracted parameters.
    ///
    /// 🎓 HOW THE FRONTEND USES THIS:
    ///   if (response.detectedReportIntent != null) {
    ///     // User asked for a report! Auto-trigger PDF generation:
    ///     const { reportCode, parameters, endpointTemplate } = response.detectedReportIntent;
    ///     // Call the report endpoint with the extracted parameters
    ///     const pdfUrl = buildReportUrl(endpointTemplate, parameters);
    ///     window.open(pdfUrl);
    ///   } else {
    ///     // Normal RAG answer — display the text response
    ///     showAnswer(response.answer);
    ///   }
    ///
    /// 🎓 WHY NULLABLE?
    /// Most RAG queries are informational questions, NOT report requests.
    /// "Which styles use cotton?" → DetectedReportIntent = null (just a question)
    /// "Generate the trim sheet for ANCHORAGE" → DetectedReportIntent = { ... }
    ///
    /// Null means "no report intent detected — this is a regular RAG answer."
    /// </summary>
    public ReportIntentDetection? DetectedReportIntent { get; init; }
}

/// <summary>
/// A reference to a source chunk that contributed to a RAG answer.
///
/// 🎓 THINK OF THIS AS A "CITATION":
/// Just like an academic paper cites its sources, our AI cites which
/// ERP records it used to generate its answer.
///
/// This lets users:
///   1. Verify the answer against the original data
///   2. Navigate to the source entity in the ERP
///   3. Understand HOW relevant each source was (via the score)
/// </summary>
public sealed class RagSourceReference
{
    /// <summary>
    /// The entity type this chunk belongs to (e.g., "Style", "PurchaseOrder").
    /// Used by the frontend to construct navigation links.
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// The entity's primary key (e.g., "S001", "PO-2024-001").
    /// Combined with EntityType, this uniquely identifies the source record.
    /// </summary>
    public required string EntityKey { get; init; }

    /// <summary>
    /// Cosine similarity score (0.0 to 1.0).
    /// Shows how relevant this chunk was to the user's question.
    ///
    /// 🎓 SCORE INTERPRETATION:
    ///   0.90+ = Highly relevant — this chunk directly answers the question
    ///   0.75–0.89 = Relevant — this chunk contains useful context
    ///   0.65–0.74 = Marginally relevant — included but may be tangential
    ///   Below 0.65 = Filtered out (below MinimumScore threshold)
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// A short preview of the chunk text (first ~150 characters).
    /// Shown in the UI as evidence/context alongside the source reference.
    ///
    /// 🎓 WHY TRUNCATE?
    /// The full chunk can be 500+ tokens. In the source list, we only need
    /// enough for the user to recognise which record it came from.
    /// If they want the full text, they navigate to the entity.
    /// </summary>
    public string ChunkPreview { get; init; } = string.Empty;
}

/// <summary>
/// API request model for RAG queries.
///
/// 🎓 WHAT THE FRONTEND SENDS:
/// POST /api/rag/query
/// {
///   "question": "Which styles have the highest fabric consumption?",
///   "entityTypeFilter": null    ← null = search everything
/// }
///
/// Or, from a specific screen:
/// {
///   "question": "What's unusual about this buyer's orders?",
///   "entityTypeFilter": "PurchaseOrder"  ← only search PO data
/// }
/// </summary>
public sealed class RagQueryRequest
{
    /// <summary>
    /// The user's natural language question.
    /// </summary>
    public required string Question { get; init; }

    /// <summary>
    /// Optional: restrict the search to a specific entity type.
    /// Null or empty searches all entity types.
    /// Valid values: "Style", "PurchaseOrder", "Buyer", "Supplier"
    /// </summary>
    public string? EntityTypeFilter { get; init; }
}

/// <summary>
/// 🆕 Represents a detected report generation intent from Claude's RAG response.
///
/// 🎓 WHAT IS THIS?
/// When the user types something like:
///   "Can I get the Trim Sheet for style ANCHORAGE?"
///   "Generate the material consumption report for order PO-2024-001"
///   "Show me the BOM report for COTTON-POLO"
///
/// Claude recognises this as a REPORT REQUEST (not a data question) and outputs
/// a structured intent block. RagService parses that block into this model.
///
/// 🎓 THE FLOW:
///   1. User asks: "Get me the trim sheet for ANCHORAGE"
///   2. RAG finds Style ANCHORAGE in the vector store (with BuyerCode, Order, etc.)
///   3. Claude sees the report catalogue in its system prompt
///   4. Claude recognises "trim sheet" matches "TrimSheet" report
///   5. Claude extracts the required parameters from the RAG context
///   6. Claude outputs |||REPORT_INTENT||| JSON block
///   7. RagService parses it into this model
///   8. Frontend receives it and calls the PDF endpoint automatically
///
/// 🎓 WHY A SEPARATE MODEL (NOT JUST A STRING)?
/// Typed models give the frontend:
///   - IntelliSense/autocomplete in TypeScript (after codegen)
///   - Compile-time safety — can't misspell "reportCode" as "reportcode"
///   - Clear contract — the frontend team knows exactly what to expect
/// </summary>
public sealed class ReportIntentDetection
{
    /// <summary>
    /// The report code from the ReportRegistry table (e.g., "TrimSheet").
    ///
    /// 🎓 THIS IS THE KEY:
    /// The frontend uses this to look up the report in the registry API
    /// (GET /api/report-registry/{reportCode}) if it needs additional metadata.
    /// But normally, everything needed is already in this model.
    /// </summary>
    public required string ReportCode { get; init; }

    /// <summary>
    /// Human-readable display name (e.g., "Trim Sheet Report").
    /// The frontend can show this in a confirmation dialog:
    ///   "Generate Trim Sheet Report for Style ANCHORAGE?"
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// The API endpoint template for generating this report (e.g., "api/trim-sheet-report/pdf").
    ///
    /// 🎓 HOW THE FRONTEND USES THIS:
    /// The endpoint template is a relative URL. The frontend combines it with
    /// the extracted parameters to build the full request:
    ///   fetch(`${baseUrl}/${endpointTemplate}?buyerCode=5&order=PO001&typeCode=1&styleCode=ANCHORAGE`)
    /// </summary>
    public required string EndpointTemplate { get; init; }

    /// <summary>
    /// The extracted parameter values, ready to pass to the report endpoint.
    ///
    /// 🎓 EXAMPLE FOR TRIM SHEET:
    /// {
    ///   "buyerCode": "5",
    ///   "order": "PO-2024-001",
    ///   "typeCode": "1",
    ///   "styleCode": "ANCHORAGE"
    /// }
    ///
    /// 🎓 WHY Dictionary&lt;string, string&gt;?
    /// Parameters vary by report — TrimSheet needs 4 params, another report
    /// might need 2 or 6. A dictionary is flexible and serialises naturally
    /// to a JSON object the frontend can iterate over.
    ///
    /// 🎓 WHY ALL STRINGS?
    /// HTTP query parameters are strings. The backend endpoint does the
    /// parsing/conversion (int.Parse for buyerCode, etc.). Keeping them
    /// as strings here avoids type conversion at this layer.
    /// </summary>
    public Dictionary<string, string> Parameters { get; init; } = new();

    /// <summary>
    /// Claude's confidence that this is truly a report request (0.0 to 1.0).
    ///
    /// 🎓 WHY INCLUDE CONFIDENCE?
    /// Sometimes the user's intent is ambiguous:
    ///   "Tell me about the trim sheet for ANCHORAGE" — is this a report request or a data question?
    ///   Confidence 0.9 → Probably wants the PDF
    ///   Confidence 0.5 → Might just want info ABOUT the trim sheet
    ///
    /// The frontend can use this threshold:
    ///   confidence >= 0.8 → Auto-generate the PDF
    ///   confidence 0.5–0.8 → Show a "Did you want to generate this report?" prompt
    ///   confidence < 0.5 → Just show the text answer (shouldn't happen — we filter these out)
    /// </summary>
    public double Confidence { get; init; }
}
