namespace ApparelPro.AI.Prompts;

/// <summary>
/// Prompt templates for RAG (Retrieval-Augmented Generation) queries.
///
/// 🎓 WHAT ARE RAG PROMPTS?
/// RAG prompts are different from direct entity prompts (Summarise/Analyse/Chat):
///
/// DIRECT prompts: "Here is the FULL entity data as JSON. Summarise it."
///   → Claude gets ONE entity's complete data
///   → Works great for focused questions about a specific record
///
/// RAG prompts: "Here are RELEVANT CHUNKS I found by searching. Answer the question."
///   → Claude gets FRAGMENTS from potentially MANY entities
///   → Works great for cross-entity, discovery-style questions
///
/// 🎓 THE CRITICAL RULE: "Only answer from the context"
/// The most important instruction in a RAG prompt is telling the AI:
///   "If the answer isn't in the provided context, say so. DO NOT make things up."
///
/// Without this rule, Claude might hallucinate plausible-sounding data.
/// With this rule, Claude will say "I don't have enough data to answer that"
/// which is MUCH better than a confident wrong answer in an ERP system.
///
/// 🎓 WHY SEPARATE FROM ChatPromptTemplates?
/// Chat prompts are designed for multi-turn conversations about a SINGLE entity.
/// RAG prompts are designed for one-shot answers across MULTIPLE entities.
/// Different job → different prompt engineering → different template class.
/// </summary>
public static class RagPromptTemplates
{
    /// <summary>
    /// System prompt for RAG-augmented queries.
    ///
    /// 🎓 PROMPT ANATOMY:
    /// 1. ROLE: "You are an ApparelPro ERP assistant"
    ///    → Sets the domain expertise (garment manufacturing terms)
    ///
    /// 2. GROUNDING RULE: "Answer ONLY from the retrieved context"
    ///    → Prevents hallucination — the #1 risk in enterprise AI
    ///
    /// 3. SOURCE CITATION: "Reference which entity your answer comes from"
    ///    → Enables traceability — users can verify against the original record
    ///
    /// 4. FORMATTING: "Be concise, use garment industry terms"
    ///    → Matches the existing chat/summarise tone
    ///
    /// 🎓 TEMPERATURE NOTE:
    /// RAG queries use Temperature = 0.2 (very low) because:
    ///   - We want FACTUAL extraction from context, not creative writing
    ///   - Lower temperature = more deterministic = more reliable
    ///   - The "creativity" comes from WHICH chunks are retrieved, not from generation
    /// </summary>
    public const string RagSystemPrompt = """
        You are an AI assistant embedded in ApparelPro, a garment and apparel manufacturing ERP system.
        You have expert knowledge of apparel manufacturing operations: costing (FOB, CMT, CIF),
        bill of materials (BOM), material consumption matrices, wastage allowances, fabric yield,
        trim procurement, cut-plan management, and supplier logistics.

        IMPORTANT — RETRIEVAL-AUGMENTED GENERATION (RAG) MODE:
        You are answering questions using RETRIEVED CONTEXT from the ApparelPro database.
        The context below contains text chunks that were found to be most relevant to the user's question.
        Each chunk is labelled with its source entity type and key.

        STRICT RULES:
        1. Answer ONLY from the retrieved context provided below. This is your ONLY source of truth.
        2. If the context does not contain enough information to answer the question, say so clearly:
           "Based on the available data, I don't have enough information to answer that question.
            You may want to check [suggest which screen/report]."
        3. NEVER fabricate data, numbers, entity keys, or records not present in the context.
        4. When citing data, reference the source entity:
           "According to Style S001..." or "Purchase Order PO-2024-001 shows..."
        5. If multiple sources contribute to the answer, mention each one.
        6. Be concise — aim for 2-4 short paragraphs. This is an ERP tool, not an essay.
        7. Use garment industry terminology naturally (consumption, wastage, BOM, FOB, etc.).
        8. Format currency to 2 decimal places and percentages to 1 decimal place.
        9. When showing calculations, state the formula briefly:
           "Total: 1,500 pcs × $4.50 FOB = $6,750.00"
        """;

    /// <summary>
    /// Builds the full user message with retrieved context and the original question.
    ///
    /// 🎓 WHY THIS FORMAT?
    /// We use XML-style tags to clearly separate the context from the question.
    /// Claude is excellent at following structured prompts with labelled sections.
    ///
    /// The format:
    ///   <retrieved_context>
    ///     [1] (Style / S001, relevance: 0.92)
    ///     Cotton T-Shirt, Buyer: NEXT, Order: ORD-2024-001...
    ///
    ///     [2] (Style / S045, relevance: 0.85)
    ///     Cotton Polo Shirt, Buyer: H&amp;M, Order: ORD-2024-015...
    ///   </retrieved_context>
    ///
    ///   <question>
    ///     Which styles use cotton fabric?
    ///   </question>
    ///
    /// 🎓 WHY INCLUDE RELEVANCE SCORES?
    /// Showing Claude the relevance score helps it:
    ///   - Weight higher-scored chunks more in its answer
    ///   - Mention lower-scored chunks as "possibly relevant" rather than definitive
    ///   - Understand which chunks are truly about the topic vs. tangentially related
    /// </summary>
    /// <param name="retrievedChunks">
    /// The formatted context chunks with their source metadata.
    /// Built by RagService from the VectorSearchResults.
    /// </param>
    /// <param name="question">The user's original question.</param>
    /// <returns>The formatted user message combining context and question.</returns>
    public static string BuildRagUserMessage(string retrievedChunks, string question)
    {
        return $"""
            <retrieved_context>
            {retrievedChunks}
            </retrieved_context>

            <question>
            {question}
            </question>
            """;
    }

    /// <summary>
    /// Builds the "no results" response when the vector store returns no relevant chunks.
    ///
    /// 🎓 WHY NOT JUST RETURN AN EMPTY STRING?
    /// When the vector store has no matches, we STILL want a helpful response:
    ///   - Acknowledges the question was understood
    ///   - Explains that no relevant data was found
    ///   - Suggests next steps (rephrase, check specific screens)
    ///
    /// We generate this CLIENT-SIDE (no AI call needed!) to:
    ///   1. Save tokens (no point sending an empty context to Claude)
    ///   2. Respond instantly (no API latency)
    ///   3. Be honest (not pretending the AI "couldn't find" something)
    /// </summary>
    /// <param name="question">The user's original question.</param>
    /// <param name="entityTypeFilter">The entity type filter that was applied (if any).</param>
    /// <returns>A helpful "no results" message.</returns>
    public static string BuildNoResultsMessage(string question, string? entityTypeFilter)
    {
        var filterNote = string.IsNullOrWhiteSpace(entityTypeFilter)
            ? "all entity types (Styles, Purchase Orders, Buyers, Suppliers)"
            : $"**{entityTypeFilter}** records";

        return $"""
            I searched across {filterNote} but couldn't find data relevant to your question:
            "{question}"

            This could mean:
            • The data hasn't been indexed yet (the sync job runs every 15 minutes)
            • Try rephrasing your question with different terms
            • Check the specific entity screen directly for the information you need

            If you recently added or modified records, they'll be available for AI queries after the next sync cycle.
            """;
    }
}
