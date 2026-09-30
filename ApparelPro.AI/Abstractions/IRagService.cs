using ApparelPro.AI.Models;

namespace ApparelPro.AI.Abstractions;

/// <summary>
/// Abstraction for the RAG (Retrieval-Augmented Generation) query pipeline.
///
/// 🎓 WHAT IS THE RAG SERVICE?
/// This is the ORCHESTRATOR — the conductor of the RAG pipeline.
/// It coordinates 4 other services to answer cross-entity questions:
///
///   User Question: "Which styles use cotton fabric?"
///         │
///         ▼
///   ┌─────────────────┐
///   │  1. EMBED        │  IEmbeddingService.EmbedAsync()
///   │  question → vector│  Convert the question to a 1536-dim vector
///   └────────┬─────────┘
///            │ [0.023, -0.041, 0.089, ...]
///            ▼
///   ┌─────────────────┐
///   │  2. SEARCH       │  IVectorStoreService.SearchAsync()
///   │  vector → chunks │  Find the most similar stored vectors in Qdrant
///   └────────┬─────────┘
///            │ [{score: 0.92, text: "Style S001: Cotton T-Shirt..."}, ...]
///            ▼
///   ┌─────────────────┐
///   │  3. BUILD CONTEXT│  Assemble the matched chunks into a text block
///   │  chunks → prompt │  that Claude can read and reference
///   └────────┬─────────┘
///            │ "RETRIEVED CONTEXT:\n[1] Style S001: Cotton T-Shirt..."
///            ▼
///   ┌─────────────────┐
///   │  4. GENERATE     │  IAiService.CompleteAsync()
///   │  context → answer│  Claude reads the context and answers grounded
///   └────────┬─────────┘
///            │ "Based on the data, two styles use cotton fabric: S001..."
///            ▼
///        RagQueryResponse
///
/// 🎓 WHY A SEPARATE SERVICE?
/// The existing IAiService handles DIRECT entity queries:
///   "Summarise THIS style" → Claude gets the full entity JSON
///
/// IRagService handles CROSS-ENTITY queries:
///   "Which suppliers are cheapest for cotton?" → needs to search across ALL suppliers
///   "Compare PO-2024-001 with PO-2024-002" → needs data from MULTIPLE entities
///
/// The key difference: direct queries know exactly which entity to look at,
/// RAG queries need to DISCOVER which entities are relevant.
///
/// 🎓 ARCHITECTURE PATTERN: Strategy + Facade
/// - Strategy: The pipeline steps are interchangeable (swap Qdrant for Pinecone)
/// - Facade: One clean method hides the complexity of 4-step orchestration
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Execute a RAG query: embed the question, search the vector store,
    /// build context from retrieved chunks, and generate a grounded answer.
    ///
    /// 🎓 PARAMETERS EXPLAINED:
    /// - question: The natural language question from the user.
    ///     "Which styles have the highest fabric consumption?"
    ///
    /// - entityTypeFilter: Optional — narrows the search to one entity type.
    ///     null → search everything (styles, POs, buyers, suppliers)
    ///     "Style" → only search style chunks
    ///     "PurchaseOrder" → only search PO chunks
    ///
    ///   This is useful when the user is on a specific screen:
    ///     On the Styles screen → filter to "Style" for faster, more relevant results
    ///     On the dashboard → no filter, search everything
    ///
    /// - cancellationToken: Standard .NET cancellation pattern.
    ///     If the user navigates away or the request times out,
    ///     this token cancels the pipeline mid-flight.
    ///
    /// 🎓 WHAT COMES BACK:
    /// RagQueryResponse contains:
    ///   - Answer: Claude's grounded response text
    ///   - Sources: Which chunks were used (entity type, key, relevance score)
    ///   - Provider/Model: Which AI generated the answer
    ///   - Token usage: For cost tracking
    ///   - HasResults: Whether the vector store found anything relevant
    ///
    /// If HasResults is false, the answer will say "I couldn't find relevant data"
    /// rather than hallucinating an answer.
    /// </summary>
    /// <param name="question">The user's natural language question.</param>
    /// <param name="entityTypeFilter">
    /// Optional: restrict search to a specific entity type (e.g., "Style", "PurchaseOrder").
    /// Null searches all entity types.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The grounded answer with source references and metadata.</returns>
    Task<RagQueryResponse> QueryAsync(
        string question,
        string? entityTypeFilter = null,
        CancellationToken cancellationToken = default);
}
