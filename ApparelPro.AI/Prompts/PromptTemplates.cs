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
    /// System prompt for entity summarisation — upgraded to include analysis and recommendations.
    /// No longer a plain narration — now produces actionable insights with risk flags.
    /// </summary>
    public const string SummariseSystem = """
        You are an AI assistant embedded in ApparelPro, a garment and apparel manufacturing ERP system
        used by merchandisers, production managers, and inventory staff.

        Your role is to produce an **insightful summary with actionable analysis** of ERP records.
        The user is looking at a specific entity and needs both a quick overview AND your expert assessment.

        You have expert knowledge of apparel manufacturing operations: costing (FOB, CMT, CIF),
        bill of materials (BOM), material consumption matrices, wastage allowances, fabric yield,
        trim procurement, cut-plan management, and supplier logistics.

        Your response must follow this structure:

        1. **OVERVIEW** — The key facts at a glance. Lead with the single most important metric
           (e.g. order value, total consumption, fulfilment status). Keep this to 3-5 lines.

        2. **KEY METRICS** — Calculate the numbers that matter from the data.
           Show formulas briefly: "Total order value: 5,000 pcs × $8.50 FOB = $42,500.00".
           Use the actual field names from the data so the user can trace your working.

        3. **RISK FLAGS** — Flag issues that need attention. Rate each as 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH.
           Be specific: "supplierCode is empty on 3 of 12 BOM lines — those materials
           can't be procured until a supplier is assigned."
           If nothing is flagged, state "No immediate risks identified."

        4. **RECOMMENDATIONS** — 2-3 specific actions the user should consider, ranked by impact.
           Each states WHAT to do and WHY it matters.

        Formatting rules:
        - Use garment/apparel industry terms: "consumption" (not usage), "wastage allowance"
          (not buffer), "FOB price" (not unit price), "BOM" (bill of materials), "CMT" (cut-make-trim).
        - Format currency to 2 decimal places with the symbol. Format percentages to 1 decimal place.
        - Use structured sections with clear headings.
        - If data seems incomplete, say what's missing — don't guess or pad.
        - Never fabricate data. Only use values present in the provided JSON.
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
    /// 🎓 User prompt template for Buyer entity summarisation.
    /// Buyers are the customers who place garment orders — they are central to every
    /// style and purchase order in the system. This template references the actual
    /// Buyer entity fields: buyerCode, name, status, telephoneNos, mobileNos, fax,
    /// cusdec (customs declaration reference), and addresses (collection).
    /// Placeholders: {0} = serialised Buyer JSON data, {1} = optional user query.
    /// </summary>
    public const string SummariseBuyer = """
        Summarise this buyer profile for a merchandiser or order-entry operator.

        The JSON contains a buyer record with fields: buyerCode (unique identifier),
        name (buyer/brand name), status (Active or Inactive), telephoneNos (landline),
        mobileNos (mobile number), fax, cusdec (customs declaration reference used
        in export documentation), and addresses (collection — each address has:
        streetAddress, city, state, postCode, countryCode, addressType
        [Residential/Postal/Corporate/Billing/Delivery], and default flag).

        Include in your summary:
        1. Buyer identity: code, name, and status. If Inactive, flag this prominently —
           an inactive buyer should not receive new orders.
        2. Contact channels: phone, mobile, fax — note which are available.
           Flag if ALL contact fields are empty (no way to reach this buyer).
        3. Addresses: list each address with its type (Corporate, Delivery, etc.).
           Flag if no addresses are on file — this blocks shipping documentation.
        4. Customs: if cusdec is set, note the customs declaration reference.
           This is used for export clearance paperwork.
        5. Data completeness: flag any critical gaps that would block order processing
           (no contact info, no delivery address, inactive status).

        Buyer Data:
        {0}

        {1}
        """;

    /// <summary>
    /// 🎓 User prompt template for StockItem entity summarisation.
    /// StockItems are the materials and trims used in garment manufacturing — fabric,
    /// buttons, zippers, thread, labels, etc. They sit under a parent Stock category
    /// (e.g., Stock "01" = Fabric, StockItem "01FB" = Cotton Poplin under Fabric).
    /// This template references: stockCode (parent category key), itemCode (item key),
    /// description (item name), and the parent stock's description (category name).
    /// Placeholders: {0} = serialised StockItem JSON data, {1} = optional user query.
    /// </summary>
    public const string SummariseStockItem = """
        Summarise this stock item (material/trim) for a merchandiser or inventory officer.

        The JSON contains a stock item with fields: stockCode (parent stock category code —
        e.g. "01" for Fabric, "02" for Buttons), itemCode (unique item identifier within
        that category — e.g. "01FB" for a specific fabric type), description (the item's
        full name/description), and stock (the parent category object with its own
        stockCode and description — e.g. "Fabric" or "Trims").

        Include in your summary:
        1. Item identity: itemCode, description, and parent stock category
           (stockCode + stock description). Format as: "Item [itemCode] — [description]
           under category [stock.description] ([stockCode])".
        2. Classification: based on the stockCode and description, identify whether
           this is a fabric, trim, accessory, packing material, or other category.
           Use garment industry terminology.
        3. Data completeness: flag if description is empty or generic.
           A well-described item helps procurement officers source accurately.
        4. Note: this is the master item record only. Consumption quantities, suppliers,
           and pricing are tracked at the style BOM level (materialConsumptionLedger).
           If the user asks about consumption or cost, explain this data lives in
           individual style records.

        Stock Item Data:
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
    /// <summary>
    /// 🎓 Resolves the correct user prompt template for a given entity type.
    /// Each entity type gets a specialised template that references its exact JSON fields,
    /// so the AI knows precisely what data structure it's working with. Unrecognised types
    /// fall through to SummariseGeneric which handles any entity shape gracefully.
    /// </summary>
    public static string GetSummariseTemplate(string entityType) =>
        entityType.ToUpperInvariant() switch
        {
            "STYLE" => SummariseStyle,
            "PURCHASEORDER" or "PO" => SummarisePurchaseOrder,
            "SUPPLIER" => SummariseSupplier,
            "BUYER" => SummariseBuyer,             // 🎓 NEW — dedicated buyer template with contact/address analysis
            "STOCKITEM" or "ITEM" => SummariseStockItem, // 🎓 NEW — material/trim master record template
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
    /// 🎓 Analysis template for Buyer entity.
    /// Deeper than summarise — assesses the buyer record's completeness, communication
    /// risk, and readiness for order processing. References actual Buyer entity fields:
    /// buyerCode, name, status, telephoneNos, mobileNos, fax, cusdec, addresses.
    /// Placeholders: {0} = serialised Buyer JSON data, {1} = optional user query.
    /// </summary>
    public const string AnalyseBuyer = """
        Perform a deep analysis of this buyer profile for merchandising and order management.

        The JSON contains: buyerCode, name, status (Active/Inactive), telephoneNos,
        mobileNos, fax, cusdec (customs declaration reference), addresses (collection —
        each with: streetAddress, city, state, postCode, countryCode, addressType
        [Residential/Postal/Corporate/Billing/Delivery], default).

        Analyse these dimensions:

        **A. Buyer Status & Order Readiness**
        - Is the buyer Active or Inactive? An Inactive buyer should NOT receive new orders.
        - Is the buyerCode in a low range (early customer, long relationship) or high
          (recently added)? Lower codes typically indicate established partnerships.
        - Is the cusdec (customs declaration reference) set? This is required for
          export documentation — missing cusdec blocks shipment clearance.

        **B. Contact Completeness & Communication Risk**
        - Count available contact channels: telephoneNos, mobileNos, fax.
        - Rate communication risk:
          🟢 LOW = 2+ channels available (phone + mobile, or phone + fax).
          🟡 MEDIUM = only 1 channel available.
          🔴 HIGH = zero contact information on file.
        - In garment manufacturing, unreachable buyers during approval windows
          (sample approval, shipment confirmation) cause costly production delays.

        **C. Address Analysis**
        - How many addresses are on file? List each with its type.
        - Is there a Corporate address? (Head office for contracts/invoicing.)
        - Is there a Delivery address? (Required for shipping documentation.)
        - Is there a Billing address? (Required for invoicing.)
        - Flag if no default address is set — the system needs one for auto-population.
        - Flag if addresses span multiple countries (multi-region buyer).

        **D. Data Gaps & Risk Assessment**
        - Rate overall buyer record as 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH risk.
        - What additional data would strengthen this record?
          (Credit terms, payment history, preferred shipping method, brand guidelines,
          quality standards, seasonal ordering patterns.)
        - Note: the buyer's order history and style portfolio are not included in
          this snapshot. A full buyer assessment would require PO and style data.

        End with 2-4 prioritised recommendations.

        Buyer Data:
        {0}

        {1}
        """;

    /// <summary>
    /// 🎓 Analysis template for StockItem entity.
    /// Analyses the material/trim master record. StockItems are lightweight reference
    /// records — the real depth (consumption, pricing, suppliers) lives in style BOMs.
    /// This template focuses on classification, naming quality, and what data would
    /// be needed for a fuller material assessment.
    /// Placeholders: {0} = serialised StockItem JSON data, {1} = optional user query.
    /// </summary>
    public const string AnalyseStockItem = """
        Perform a deep analysis of this stock item (material/trim) for inventory
        and procurement strategy.

        The JSON contains: stockCode (parent stock category code), itemCode (unique
        item identifier), description (item name), stock (parent category object with
        stockCode and description).

        Note: This is the master item record only. Consumption data, supplier assignments,
        pricing, and wastage allowances are tracked at the style BOM level
        (materialConsumptionLedger). Analyse what's available and clearly state what
        additional data would strengthen the analysis.

        Analyse these dimensions:

        **A. Item Classification**
        - Identify the material category from stockCode and stock.description:
          Is this a fabric, trim, accessory, packing material, label, or other?
        - Based on the itemCode pattern and description, assess whether this is a
          commodity item (basic thread, standard buttons) or a specialty item
          (custom-dyed fabric, branded labels) — specialty items typically have
          longer lead times and fewer supplier options.
        - Use garment industry terminology: "shell fabric" (main body), "lining",
          "interlining", "fusible", "trim" (buttons/zippers/hooks), "packing"
          (polybags, hangers, cartons), "labels" (care/size/brand labels).

        **B. Description Quality**
        - Is the description specific enough for procurement?
          Good: "100% Cotton Poplin 58\" 120GSM" — specifies composition, width, weight.
          Poor: "Fabric" — too vague to source accurately.
        - Does it include key attributes a buyer would need?
          For fabrics: composition, width, weight (GSM), weave/knit type.
          For trims: material, size, colour, finish.
        - Flag if description is empty, generic, or uses only a code — recommend
          enriching it with sourcing-relevant details.

        **C. Code Structure Analysis**
        - Does the itemCode follow a logical pattern within its stockCode group?
          (e.g., "01FB", "01LN" under stock "01" = Fabric → FB=Fabric Body, LN=Lining.)
        - Is the stockCode/itemCode combination clear enough for warehouse staff
          to locate and issue the correct material?

        **D. Data Gaps & Recommendations**
        - What additional data would be needed for a full material assessment?
          (Preferred suppliers, unit cost range, minimum order quantity, lead time,
          available colours/widths, quality grade, country of origin,
          stock-on-hand across warehouses.)
        - Rate the item record's completeness as 🟢 / 🟡 / 🔴.
        - Recommend 2-4 actions to strengthen this record.

        Stock Item Data:
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
    /// <summary>
    /// 🎓 Resolves the correct analysis template for a given entity type.
    /// Analysis templates go deeper than summarise — they guide the AI to calculate
    /// metrics, identify risks, and produce prioritised recommendations.
    /// </summary>
    public static string GetAnalyseTemplate(string entityType) =>
        entityType.ToUpperInvariant() switch
        {
            "STYLE" => AnalyseStyle,
            "PURCHASEORDER" or "PO" => AnalysePurchaseOrder,
            "SUPPLIER" => AnalyseSupplier,
            "BUYER" => AnalyseBuyer,               // 🎓 NEW — contact risk + order readiness assessment
            "STOCKITEM" or "ITEM" => AnalyseStockItem, // 🎓 NEW — material classification + description quality
            _ => AnalyseGeneric
        };
}
