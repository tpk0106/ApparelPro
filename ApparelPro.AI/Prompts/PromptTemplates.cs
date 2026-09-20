namespace ApparelPro.AI.Prompts;

/// <summary>
/// Centralised prompt templates for ApparelPro AI features.
/// Keep prompts here — not scattered across services — so they're easy to tune.
/// </summary>
public static class PromptTemplates
{
    // ─────────────────────────────────────────────
    //  SUMMARISE MODE — quick overview (existing)
    // ─────────────────────────────────────────────

    /// <summary>
    /// System prompt for entity summarisation.
    /// </summary>
    public const string SummariseSystem = """
        You are an AI assistant for ApparelPro, a garment and apparel manufacturing ERP system.
        You help merchandisers, production managers, and inventory staff understand their data quickly.

        Rules:
        - Be concise and factual. Use bullet points for key metrics.
        - Highlight anything unusual (e.g. missing materials, cost outliers, overdue events).
        - Use garment industry terminology where appropriate.
        - Format currency values to 2 decimal places.
        - If data seems incomplete, note what's missing rather than guessing.
        - Never fabricate data that isn't in the provided context.
        """;

    /// <summary>
    /// User prompt template for Style entity summarisation.
    /// Placeholders: {0} = serialised Style JSON data, {1} = optional user query.
    /// </summary>
    public const string SummariseStyle = """
        Summarise the following garment style for a merchandiser.
        Include: buyer, order details, material count, total cost breakdown,
        any pending events, and material consumption status.

        Style Data:
        {0}

        {1}
        """;

    /// <summary>
    /// User prompt template for Purchase Order summarisation.
    /// Placeholders: {0} = serialised PO JSON data, {1} = optional user query.
    /// </summary>
    public const string SummarisePurchaseOrder = """
        Summarise the following purchase order for a merchandiser.
        Include: supplier, order value, currency, delivery status,
        item breakdown, and any outstanding quantities.

        Purchase Order Data:
        {0}

        {1}
        """;

    /// <summary>
    /// User prompt template for Supplier summarisation.
    /// Placeholders: {0} = serialised Supplier JSON data, {1} = optional user query.
    /// </summary>
    public const string SummariseSupplier = """
        Summarise the following supplier profile.
        Include: contact details, active purchase orders, delivery performance,
        and material categories supplied.

        Supplier Data:
        {0}

        {1}
        """;

    /// <summary>
    /// Generic entity summarisation fallback.
    /// Placeholders: {0} = entity type, {1} = serialised data, {2} = optional user query.
    /// </summary>
    public const string SummariseGeneric = """
        Summarise the following {0} record from a garment manufacturing ERP system.
        Provide a clear, concise overview highlighting the most important information.

        {0} Data:
        {1}

        {2}
        """;

    /// <summary>
    /// Resolves the correct user prompt template for a given entity type.
    /// </summary>
    public static string GetSummariseTemplate(string entityType) =>
        entityType.ToUpperInvariant() switch
        {
            "STYLE" => SummariseStyle,
            "PURCHASEORDER" or "PO" => SummarisePurchaseOrder,
            "SUPPLIER" => SummariseSupplier,
            _ => SummariseGeneric
        };

    // ─────────────────────────────────────────────
    //  ANALYSE MODE — deep insights (Phase 1 new)
    // ─────────────────────────────────────────────

    /// <summary>
    /// System prompt for deep entity analysis.
    /// Sets a senior analyst persona that produces actionable insights.
    /// </summary>
    public const string AnalyseSystem = """
        You are a senior garment industry analyst embedded in ApparelPro, an apparel manufacturing ERP system.
        You provide deep, actionable analysis — not summaries — to help merchandisers and production managers
        make better decisions.

        Your analysis must:
        1. QUANTIFY: Calculate ratios, percentages, cost-per-unit, margins, and variances.
           Show the math briefly (e.g. "Fabric cost $2.40/unit × 5,000 units = $12,000").
        2. COMPARE: Benchmark against garment industry norms where applicable
           (e.g. typical fabric wastage 3-5%, standard lead times, acceptable reject rates).
        3. FLAG RISKS: Identify cost outliers, overdue milestones, missing materials,
           single-supplier dependencies, or capacity bottlenecks.
        4. RECOMMEND: End with 2-4 specific, actionable recommendations ranked by impact.
           Each recommendation should state WHAT to do, WHY it matters, and the expected IMPACT.

        Formatting rules:
        - Use structured sections with clear headings.
        - Use tables for comparative data where helpful.
        - Format all currency to 2 decimal places with currency symbol.
        - Format percentages to 1 decimal place.
        - If data seems incomplete, state what's missing and what you'd need for a fuller analysis.
        - Never fabricate data. Only derive calculations from provided data.
        """;

    /// <summary>
    /// Analysis template for Style entity.
    /// Placeholders: {0} = serialised Style JSON data, {1} = optional user query.
    /// </summary>
    public const string AnalyseStyle = """
        Perform a deep analysis of the following garment style for a merchandiser making production decisions.

        Analyse these dimensions:
        - **Cost Structure**: Break down material costs per unit, identify the top 3 cost drivers,
          calculate material cost as % of total style cost.
        - **Material Readiness**: What % of required materials have been consumed/allocated?
          Flag any materials with zero consumption that should have started.
        - **Event/Milestone Status**: Are any events overdue or at risk? Calculate days remaining
          vs expected completion.
        - **Consumption Efficiency**: Compare planned vs actual consumption where data allows.
          Flag wastage above 5%.
        - **Risk Assessment**: Rate overall style risk as LOW / MEDIUM / HIGH with justification.

        End with 2-4 prioritised recommendations.

        Style Data:
        {0}

        {1}
        """;

    /// <summary>
    /// Analysis template for Purchase Order.
    /// Placeholders: {0} = serialised PO JSON data, {1} = optional user query.
    /// </summary>
    public const string AnalysePurchaseOrder = """
        Perform a deep analysis of the following purchase order for procurement decision-making.

        Analyse these dimensions:
        - **Value Analysis**: Total order value, average unit cost per item, cost distribution across items.
        - **Delivery Performance**: Are deliveries on schedule? Calculate days overdue or ahead
          for each line item. Flag items with >7 days delay.
        - **Quantity Reconciliation**: Compare ordered vs delivered vs outstanding quantities.
          Calculate fulfilment rate (%).
        - **Supplier Dependency**: Is this order concentrated with one supplier? What's the risk
          if delivery fails?
        - **Currency Exposure**: If multi-currency, note the exchange rate risk.

        End with 2-4 prioritised recommendations.

        Purchase Order Data:
        {0}

        {1}
        """;

    /// <summary>
    /// Analysis template for Supplier.
    /// Placeholders: {0} = serialised Supplier JSON data, {1} = optional user query.
    /// </summary>
    public const string AnalyseSupplier = """
        Perform a deep analysis of the following supplier profile for procurement strategy.

        Analyse these dimensions:
        - **Reliability Profile**: Based on active/completed POs, assess delivery consistency.
          Calculate on-time delivery rate if data allows.
        - **Material Coverage**: What material categories does this supplier cover?
          Are there categories where they're your sole source?
        - **Order Concentration**: What % of your total procurement flows through this supplier?
          Flag if >30% (single-point-of-failure risk).
        - **Commercial Terms**: Analyse payment terms, credit limits, pricing patterns across orders.
        - **Capacity Indicators**: Based on order volumes and frequencies, estimate supplier capacity utilisation.

        End with 2-4 prioritised recommendations.

        Supplier Data:
        {0}

        {1}
        """;

    /// <summary>
    /// Generic analysis fallback.
    /// Placeholders: {0} = entity type, {1} = serialised data, {2} = optional user query.
    /// </summary>
    public const string AnalyseGeneric = """
        Perform a deep analysis of the following {0} record from a garment manufacturing ERP system.

        Provide:
        - Key metrics and calculated ratios from the data.
        - Any anomalies, risks, or items requiring attention.
        - Comparison to garment industry benchmarks where applicable.
        - 2-4 specific, actionable recommendations ranked by impact.

        {0} Data:
        {1}

        {2}
        """;

    /// <summary>
    /// Resolves the correct analysis template for a given entity type.
    /// </summary>
    public static string GetAnalyseTemplate(string entityType) =>
        entityType.ToUpperInvariant() switch
        {
            "STYLE" => AnalyseStyle,
            "PURCHASEORDER" or "PO" => AnalysePurchaseOrder,
            "SUPPLIER" => AnalyseSupplier,
            _ => AnalyseGeneric
        };
}
