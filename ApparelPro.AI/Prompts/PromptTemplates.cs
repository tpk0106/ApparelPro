namespace ApparelPro.AI.Prompts;

/// <summary>
/// Centralised prompt templates for ApparelPro AI features.
/// Keep prompts here — not scattered across services — so they're easy to tune.
///
/// Prompt tuning notes (2024):
/// - Every template references actual JSON field names from the entity data models
///   so the AI knows exactly where to find each value.
/// - Garment manufacturing terminology is used throughout.
/// - Templates guide the AI to perform specific calculations from the available fields.
/// </summary>
public static class PromptTemplates
{
    // ─────────────────────────────────────────────
    //  SUMMARISE MODE — quick overview
    // ─────────────────────────────────────────────

    /// <summary>
    /// System prompt for entity summarisation.
    /// </summary>
    public const string SummariseSystem = """
        You are an AI assistant embedded in ApparelPro, a garment and apparel manufacturing ERP system
        used by merchandisers, production managers, and inventory staff.

        Your role is to produce concise, scannable summaries of ERP records.
        The user is looking at a specific entity in the system and wants a quick overview.

        Response rules:
        - Lead with the single most important fact (e.g. order value, fulfilment %, critical shortage).
        - Use short paragraphs or a compact bullet list — no more than 8 bullets.
        - Use garment/apparel industry terms: "consumption" (not usage), "wastage allowance"
          (not buffer), "FOB price" (not unit price), "BOM" (bill of materials), "CMT" (cut-make-trim).
        - Format currency to 2 decimal places with the symbol. Format percentages to 1 decimal place.
        - Flag anything unusual: zero consumption on materials that should have started, missing
          supplier assignments, quantities that don't reconcile.
        - If data seems incomplete, say what's missing in one line — don't guess or pad.
        - Never fabricate data. Only use values present in the provided JSON.
        - Keep the total response under 250 words.
        """;

    /// <summary>
    /// User prompt template for Style entity summarisation.
    /// Placeholders: {0} = serialised Style JSON data, {1} = optional user query.
    /// </summary>
    public const string SummariseStyle = """
        Summarise this garment style for a merchandiser who needs a quick status check.

        The JSON below contains:
        • styleDetails — the style master record with fields: buyerCode, buyer, order, orderDate,
          typeCode, styleCode, unit, quantity (order qty), unitPrice (FOB per unit), colorRatio,
          sizeRatio, hasSupplierPurchaseOrder, approvedDate, productionEndDate, exported.
        • buyerName — the buyer's display name.
        • materialConsumptionLedger — the BOM / material consumption matrix. Each row has:
          stockCode, itemCode, color, size, feature1-4, storeCode, consumptionUnit, itemUnit,
          quantityPerGarment, supplierCode, totalConsumption, percentageAllowance (wastage %),
          isAdditionalCost (true = overhead, not fabric/trim), calculateConsumption.
        • consumptionLineCount — total rows in the consumption ledger.

        Include in your summary:
        1. Style identity: buyer name, order, style code, garment type, order quantity & FOB price.
        2. Material overview: how many BOM lines, split between direct materials and additional costs.
           Total planned consumption = sum of totalConsumption across direct material rows.
        3. Supplier coverage: which materials have a supplierCode assigned vs "unassigned" (empty).
        4. Wastage: average percentageAllowance across direct materials. Flag any row > 8%.
        5. Status flags: is the style exported? Does it have supplier POs raised?

        Style Data:
        {0}

        {1}
        """;

    /// <summary>
    /// User prompt template for Purchase Order summarisation.
    /// Placeholders: {0} = serialised PO JSON data, {1} = optional user query.
    /// </summary>
    public const string SummarisePurchaseOrder = """
        Summarise this purchase order for a merchandiser who needs a quick status check.

        The JSON contains a purchase order with fields: buyerCode, order, orderDate,
        garmentType, garmentTypeName, description, buyer (buyer name), countryCode, unitCode,
        totalQuantity, currencyCode, season, basisCode, basisValue,
        quantityOverriddenBy, quantityOverriddenAt.

        Include in your summary:
        1. PO identity: buyer, order reference, garment type, season.
        2. Order value: totalQuantity × basisValue in the stated currency, plus the basis code
           (e.g. FOB, CIF, CMT — this is the pricing basis).
        3. Country and delivery: destination country, unit of measure.
        4. Quantity overrides: if quantityOverriddenBy is set, note who overrode and when.
        5. Any gaps: missing description, zero quantities, or other anomalies.

        Purchase Order Data:
        {0}

        {1}
        """;

    /// <summary>
    /// User prompt template for Supplier summarisation.
    /// Placeholders: {0} = serialised Supplier JSON data, {1} = optional user query.
    /// </summary>
    public const string SummariseSupplier = """
        Summarise this supplier profile for a merchandiser or procurement officer.

        The JSON contains a supplier with fields: supplierCode, name, telephoneNos,
        mobileNos, fax, addressId, addresses (collection with address details).

        Include in your summary:
        1. Supplier identity: code, name, contact numbers.
        2. Address: format the address naturally if available.
        3. Communication channels: phone, mobile, fax — flag if all are empty (no contact info on file).
        4. Note: the supplier's order history and material categories are not included in this
           snapshot. If the user asks about delivery performance or order volume, explain that
           this data would need to be loaded separately.

        Supplier Data:
        {0}

        {1}
        """;

    /// <summary>
    /// Generic entity summarisation fallback.
    /// Placeholders: {0} = entity type, {1} = serialised data, {2} = optional user query.
    /// </summary>
    public const string SummariseGeneric = """
        Summarise the following {0} record from ApparelPro, a garment manufacturing ERP system.
        Provide a clear, concise overview of the most important fields.
        Use garment industry terminology where appropriate.
        Flag any anomalies or missing data.

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
    //  ANALYSE MODE — deep insights
    // ─────────────────────────────────────────────

    /// <summary>
    /// System prompt for deep entity analysis.
    /// Sets a senior analyst persona that produces actionable insights.
    /// </summary>
    public const string AnalyseSystem = """
        You are a senior garment industry analyst embedded in ApparelPro,
        an apparel manufacturing ERP system. You provide deep, actionable analysis —
        not summaries — to help merchandisers and production managers make better decisions.

        You have expert knowledge of apparel supply chain operations: costing (FOB, CMT, CIF),
        bill of materials (BOM) management, material consumption planning, wastage control,
        cut-plan optimisation, fabric yield, trim procurement, and supplier management.

        Your analysis must follow this structure:

        1. KEY METRICS — Calculate and present the numbers that matter.
           Show the formula briefly: "Fabric cost: quantityPerGarment × unitPrice = $X.XX/unit".
           Use the actual field names from the data so the user can trace your working.

        2. FINDINGS — What the data reveals. Group by theme (cost, materials, timeline, risk).
           Each finding should state: the fact, why it matters, and the magnitude.

        3. RISK FLAGS — Rate each risk as 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH.
           Be specific: "supplierCode is empty on 3 of 12 BOM lines — those materials
           can't be procured until a supplier is assigned."

        4. RECOMMENDATIONS — 2–4 actions ranked by impact. Each one states:
           WHAT to do, WHY it matters, and the expected IMPACT in measurable terms.

        Formatting rules:
        - Use structured sections with clear headings.
        - Use tables for comparative data.
        - Format all currency to 2 decimal places with currency symbol.
        - Format percentages to 1 decimal place.
        - If data is incomplete, state what's missing and what you'd need for a fuller analysis.
        - Never fabricate data. Only derive calculations from provided data.
        """;

    /// <summary>
    /// Analysis template for Style entity.
    /// Placeholders: {0} = serialised Style JSON data, {1} = optional user query.
    /// </summary>
    public const string AnalyseStyle = """
        Perform a deep analysis of this garment style. The user is a merchandiser
        making production and costing decisions.

        The JSON below contains:
        • styleDetails — the style master: buyerCode, buyer, order, orderDate, typeCode,
          styleCode, unit, quantity (order qty), unitPrice (FOB/unit), colorRatio, sizeRatio,
          hasSupplierPurchaseOrder, approvedDate, productionEndDate, exported.
        • buyerName — buyer display name.
        • materialConsumptionLedger — the BOM. Each row: stockCode, itemCode, color, size,
          feature1-4, storeCode, consumptionUnit, itemUnit, quantityPerGarment, supplierCode,
          totalConsumption, percentageAllowance (wastage %), isAdditionalCost, calculateConsumption.
        • consumptionLineCount — total BOM rows.

        Analyse these dimensions:

        **A. Cost Structure**
        - Calculate total material cost per garment: for each direct material row (isAdditionalCost=false),
          quantityPerGarment contributes to per-unit material consumption.
        - Total planned consumption = sum of totalConsumption for all direct rows.
        - Identify the top 3 material lines by totalConsumption (highest cost drivers).
        - Calculate material cost as a percentage context if unitPrice (FOB) is available.
        - Separate additional costs (isAdditionalCost=true) and total them.

        **B. Material Readiness & BOM Completeness**
        - Count: total BOM lines, direct materials vs additional costs.
        - For direct materials: how many have supplierCode assigned vs blank/unassigned?
          Unassigned suppliers = procurement risk.
        - Flag any direct material where totalConsumption = 0 but calculateConsumption = true
          (planned but not yet calculated — potential gap).
        - Flag any unusually high quantityPerGarment values (context: typical fabric ~1.5-2.5 yds/garment,
          trims ~0.5-5 units/garment depending on item).

        **C. Wastage Analysis**
        - List percentageAllowance for each direct material.
        - Industry benchmark: fabric wastage 3-5%, trim wastage 2-3%.
        - Flag any material with allowance > 8% (potential over-estimation or known difficult fabric).
        - Flag any material with allowance = 0% on calculateConsumption=true rows (unrealistic).

        **D. Production Readiness**
        - Is the style approved (approvedDate set)?
        - Is the production end date set? If so, calculate days remaining from today.
        - Are supplier POs raised (hasSupplierPurchaseOrder)?
        - Is the order exported?

        **E. Risk Assessment**
        Rate overall style risk as 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH with justification.

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
        Perform a deep analysis of this purchase order for procurement decision-making.

        The JSON contains: buyerCode, order, orderDate, garmentType, garmentTypeName,
        description, buyer, countryCode, unitCode, totalQuantity, currencyCode, season,
        basisCode (pricing basis: FOB/CIF/CMT), basisValue (price per unit),
        quantityOverriddenBy, quantityOverriddenAt.

        Analyse these dimensions:

        **A. Order Value**
        - Calculate total order value: totalQuantity × basisValue in the stated currency.
        - State the pricing basis (basisCode) and what it includes
          (FOB = Free on Board, CIF = Cost Insurance Freight, CMT = Cut Make Trim).

        **B. Order Profile**
        - Garment type, season, destination country.
        - Order date and how long ago it was placed.
        - Unit of measure context (PCS = pieces, DZN = dozens — convert if needed).

        **C. Quantity Integrity**
        - Has quantity been overridden? If so, by whom and when?
        - Flag if totalQuantity is unusually low (<100) or high (>100,000) for context.
        - Flag if basisValue seems out of range for garment FOB
          (typical range $3-50/unit depending on garment type).

        **D. Data Completeness**
        - Is description filled in?
        - Is the buyer name resolved?
        - Any fields that are null/empty that should have values?

        **E. Risk Assessment**
        Rate as 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH.

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
        Perform a deep analysis of this supplier profile for procurement strategy.

        The JSON contains: supplierCode, name, telephoneNos, mobileNos, fax,
        addressId, addresses (address collection).

        Note: This snapshot contains the supplier master record only. Order history,
        delivery performance, and material category data are not included.
        Analyse what's available and clearly state what additional data would
        strengthen the analysis.

        Analyse these dimensions:

        **A. Contact Completeness**
        - Does the supplier have phone, mobile, and fax on file?
        - Is there a physical address? Multiple communication channels reduce
          risk of losing contact during critical procurement windows.
        - Flag if contact information is sparse (single channel only).

        **B. Supplier Profile Assessment**
        - Based on the supplier code range, is this an early or recent supplier
          in the system? (Lower codes typically = longer relationship.)
        - Any naming patterns that suggest the supplier's specialisation?

        **C. Data Gaps & Recommendations**
        - What data would be needed for a full supplier assessment?
          (Active PO history, on-time delivery rate, material categories supplied,
          payment terms, credit limit, quality rejection rate.)
        - Recommend 2-4 actions to strengthen this supplier record.

        Supplier Data:
        {0}

        {1}
        """;

    /// <summary>
    /// Generic analysis fallback.
    /// Placeholders: {0} = entity type, {1} = serialised data, {2} = optional user query.
    /// </summary>
    public const string AnalyseGeneric = """
        Perform a deep analysis of this {0} record from ApparelPro,
        a garment and apparel manufacturing ERP system.

        Provide:
        1. Key metrics and calculated ratios from the available data fields.
        2. Anomalies, risks, or items requiring attention — flag each as 🟢/🟡/🔴.
        3. Comparison to garment industry benchmarks where applicable.
        4. 2-4 specific, actionable recommendations ranked by impact.

        Use garment industry terminology. Show your calculations.
        Never fabricate data not present in the JSON.

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
