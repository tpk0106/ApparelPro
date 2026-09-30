using apparelPro.BusinessLogic.Services.Models.AI.IReportRegistryService;

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
///
/// 🎓 WHAT CHANGED — REPORT INTENT DETECTION:
/// The system prompt now includes a "report catalogue" section built from the
/// ReportRegistry table. This tells Claude which reports exist, their aliases
/// (CommonNames), and what parameters they need. When the user asks for a report,
/// Claude outputs a structured |||REPORT_INTENT||| block that the backend parses.
/// </summary>
public static class RagPromptTemplates
{
    /// <summary>
    /// 🎓 BASE system prompt for RAG-augmented queries.
    ///
    /// This is the STATIC part of the system prompt that never changes.
    /// The DYNAMIC part (report catalogue) gets appended at runtime
    /// by BuildFullSystemPrompt().
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
    /// </summary>
    private const string RagSystemPromptBase = """
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
    /// 🎓 KEPT FOR BACKWARD COMPATIBILITY:
    /// The original static RagSystemPrompt is still available for any code
    /// that references it directly. However, RagService now uses
    /// BuildFullSystemPrompt() which appends the report catalogue.
    ///
    /// If you have other code referencing RagPromptTemplates.RagSystemPrompt,
    /// it will still work — it just won't include the report detection instructions.
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
    /// 🆕 Builds the FULL system prompt by combining the base RAG prompt
    /// with the dynamic report catalogue section.
    ///
    /// 🎓 WHY BUILD DYNAMICALLY?
    /// The report catalogue comes from the ReportRegistry database table.
    /// New reports can be added without changing any code — just INSERT a row
    /// into ReportRegistries and the next RAG query will include it.
    ///
    /// This is the HYBRID APPROACH we designed:
    ///   - Database (ReportRegistry) = the CATALOGUE (which reports exist, their aliases)
    ///   - Claude's NLU = the MATCHING (understanding "give me the BOM" means TrimSheet)
    ///
    /// 🎓 WHEN reportCatalogue IS EMPTY:
    /// If no active reports exist (or the service couldn't load them),
    /// we fall back to the base prompt without report detection.
    /// The RAG query still works — it just won't detect report intents.
    /// </summary>
    /// <param name="reportCatalogue">
    /// The formatted report catalogue string from BuildReportCatalogueBlock().
    /// Pass null or empty to use the base prompt without report detection.
    /// </param>
    /// <returns>The complete system prompt with report detection instructions.</returns>
    public static string BuildFullSystemPrompt(string? reportCatalogue)
    {
        if (string.IsNullOrWhiteSpace(reportCatalogue))
        {
            return RagSystemPromptBase;
        }

        // 🎓 PROMPT STRUCTURE:
        // [Base RAG instructions]
        // [Report catalogue + detection instructions]
        //
        // The report detection section comes AFTER the base rules so Claude
        // processes the grounding rules first, then the report detection rules.
        // Order matters in prompts — earlier instructions carry more weight.
        return $"""
            {RagSystemPromptBase}

            {reportCatalogue}
            """;
    }

    /// <summary>
    /// 🆕 Builds the report catalogue section of the system prompt from active ReportRegistry entries.
    ///
    /// 🎓 WHAT THIS PRODUCES:
    /// A block of text that tells Claude:
    ///   1. Which reports are available in the system
    ///   2. What aliases/synonyms each report goes by (CommonNames)
    ///   3. What parameters each report needs
    ///   4. How to output a structured intent block when the user requests a report
    ///
    /// 🎓 EXAMPLE OUTPUT:
    ///
    ///   AVAILABLE REPORTS:
    ///   You can detect when the user is requesting one of these reports:
    ///
    ///   - Report Code: TrimSheet
    ///     Display Name: Trim Sheet Report
    ///     Description: Material consumption and costing breakdown for a style...
    ///     Also known as: trim sheet, material consumption, BOM report, bill of materials...
    ///     Required Parameters: buyerCode, order, typeCode, styleCode
    ///     Parameter Sources: buyerCode → Style.BuyerCode (int), order → Style.Order (string)...
    ///
    ///   REPORT DETECTION RULES:
    ///   [instructions for Claude to output |||REPORT_INTENT||| block]
    ///
    /// 🎓 WHY CommonNames MATTER:
    /// Users don't always use the exact report name. They might say:
    ///   "Get me the BOM for ANCHORAGE" (BOM = Bill of Materials = Trim Sheet)
    ///   "Show the material consumption for style POLO-01" (material consumption = Trim Sheet)
    ///   "I need the costing sheet for order PO-2024-001" (costing sheet = Trim Sheet)
    ///
    /// By listing CommonNames in the prompt, Claude's own NLU understands the synonyms.
    /// We DON'T do keyword matching — Claude handles the fuzzy matching naturally.
    /// </summary>
    /// <param name="activeReports">The active report registry entries from the database.</param>
    /// <returns>The formatted report catalogue block, or empty string if no reports.</returns>
    public static string BuildReportCatalogueBlock(
        IEnumerable<ReportRegistryServiceModel> activeReports)
    {
        var reports = activeReports.ToList();

        if (reports.Count == 0)
        {
            return string.Empty;
        }

        // ── Build the report listing ────────────────────
        var reportEntries = new List<string>();

        foreach (var report in reports)
        {
            // 🎓 Each report entry gives Claude everything it needs:
            //   - The code (for the intent output)
            //   - The display name (for natural language responses)
            //   - The description (for disambiguation — "consumption" in what context?)
            //   - Common names (the synonym list for fuzzy matching)
            //   - Required parameters (what Claude needs to extract from context)
            //   - Parameter sources (WHERE to find each param in the RAG chunks)

            var entry = $"""
                  - Report Code: {report.ReportCode}
                    Display Name: {report.DisplayName}
                    Description: {report.Description}
                """;

            // 🎓 Only include CommonNames if they exist
            if (!string.IsNullOrWhiteSpace(report.CommonNames))
            {
                entry += $"\n    Also known as: {report.CommonNames}";
            }

            // 🎓 Show required params so Claude knows what to extract
            if (!string.IsNullOrWhiteSpace(report.RequiredParams))
            {
                entry += $"\n    Required Parameters (JSON): {report.RequiredParams}";
            }

            // 🎓 ParamSources tells Claude WHERE each parameter comes from
            // in the RAG context chunks (which entity field maps to which param)
            if (!string.IsNullOrWhiteSpace(report.ParamSources))
            {
                entry += $"\n    Parameter Sources: {report.ParamSources}";
            }

            reportEntries.Add(entry);
        }

        var reportListing = string.Join("\n\n", reportEntries);

        // ── Build the full catalogue block with detection rules ──
        // 🎓 THIS IS THE KEY PROMPT ENGINEERING:
        // We give Claude VERY SPECIFIC instructions on:
        //   1. When to detect a report intent (vs. just answering a question)
        //   2. How to extract parameters from the retrieved context
        //   3. The EXACT format to output the intent block
        //   4. How to estimate confidence
        //
        // 🎓 THE |||REPORT_INTENT||| MARKER:
        // We use a distinctive delimiter that would NEVER appear in normal text.
        // Triple pipes + uppercase text is easy to parse and impossible to confuse
        // with regular content. The backend splits on this marker to extract the JSON.
        // 🎓 WHY $$""" (DOUBLE-DOLLAR RAW STRING)?
        // The JSON example below contains literal { and } braces.
        // With single $""", we'd need {{ to escape every brace — but the NESTED
        // braces in "parameters": { ... } inside the outer { ... } create {{
        // patterns that the compiler misreads as interpolation expressions.
        //
        // Solution: $$""" makes {{ the interpolation trigger instead of {.
        // Now single { and } are literal (perfect for JSON examples),
        // and we use {{reportListing}} for the one interpolated variable.
        return $$"""

            AVAILABLE REPORTS:
            The ApparelPro system can generate the following reports. When the user requests
            one of these reports (using the report name OR any of its aliases), you should
            detect this as a REPORT INTENT and include the structured output described below.

            {{reportListing}}

            REPORT INTENT DETECTION RULES:
            When you determine the user is requesting a report (not just asking a question about data),
            follow these steps:

            1. IDENTIFY the report: Match the user's request to one of the reports listed above.
               Use your natural language understanding — the user might say "trim sheet", "BOM",
               "material consumption report", "costing breakdown", etc. All of these map to TrimSheet.

            2. EXTRACT parameters: Look at the Required Parameters for that report. Find each
               parameter's value in the retrieved context chunks using the Parameter Sources mapping.
               For example, if the user asks about "style ANCHORAGE" and the context contains
               a Style chunk with BuyerCode=5, Order="PO-2024-001", TypeCode=1, StyleCode="ANCHORAGE",
               extract all four values.

            3. ASSESS confidence: Rate your confidence (0.0 to 1.0) that this is truly a report request:
               - 0.9-1.0: Clear report request ("Generate the trim sheet for ANCHORAGE")
               - 0.7-0.8: Likely report request ("Can I see the BOM for this style?")
               - 0.5-0.6: Ambiguous ("Tell me about the trim sheet" — might want info, not the PDF)
               - Below 0.5: Probably just a question — do NOT output intent block

            4. OUTPUT: If confidence >= 0.5, include BOTH:
               a) A natural language response confirming what you found and that you can generate the report
               b) The intent block in this EXACT format (on its own line, after your text response):

            |||REPORT_INTENT|||
            {
              "reportCode": "TrimSheet",
              "displayName": "Trim Sheet Report",
              "confidence": 0.95,
              "parameters": {
                "buyerCode": "5",
                "order": "PO-2024-001",
                "typeCode": "1",
                "styleCode": "ANCHORAGE"
              }
            }
            |||END_REPORT_INTENT|||

            IMPORTANT RULES FOR REPORT INTENT:
            - ALL parameter values must be strings (even numbers — "5" not 5)
            - If you cannot find ALL required parameters in the context, do NOT output the intent block.
              Instead, tell the user which parameters are missing and suggest they provide them.
            - The intent block must be valid JSON between the |||REPORT_INTENT||| markers.
            - Do NOT output the intent block for general questions about reports
              (e.g., "What reports are available?" or "What does the trim sheet show?").
              Only output it when the user wants to GENERATE/VIEW/GET a specific report for a specific entity.
            - Always include a text response BEFORE the intent block — never output ONLY the marker.
            """;
    }

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
