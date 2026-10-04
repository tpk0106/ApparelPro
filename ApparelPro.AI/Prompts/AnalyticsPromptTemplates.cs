namespace ApparelPro.AI.Prompts;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 CROSS-ENTITY ANALYTICS PROMPT TEMPLATES
// Phase 2: Cross-Entity Analytics — prompts that guide the AI to analyse
// data ACROSS multiple entities, not just a single record.
//
// 🎓 WHY A SEPARATE FILE?
// PromptTemplates.cs handles single-entity Summarise/Analyse (Phase 1).
// Cross-entity analytics uses a fundamentally different prompting strategy:
//   - Single-entity: "Here's one Style JSON — analyse it."
//   - Cross-entity: "Here's aggregated data across 50 styles — find patterns,
//     outliers, and trends across the whole dataset."
//
// The system prompt is different (strategic analyst, not record reviewer),
// the user prompt structure is different (aggregated data, not one record),
// and the response structure is different (trends/rankings/comparisons,
// not findings/risks for one item).
//
// 🎓 SUPPORTED ANALYTICS CATEGORIES:
// Each category maps to a different data aggregation query in the service layer
// and a specialised prompt template that tells the AI exactly what data shape
// to expect and what analysis dimensions to explore.
//
//   MATERIAL_CONSUMPTION — consumption patterns, wastage, cost drivers across styles
//   SUPPLIER_PERFORMANCE  — supplier coverage, order distribution, contact completeness
//   BUYER_PORTFOLIO       — buyer order volumes, style counts, revenue concentration
//   ORDER_PIPELINE        — purchase order status, value distribution, timeline analysis
//   COST_ANALYSIS         — cost structures, pricing patterns, margin indicators
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Prompt templates for Phase 2: Cross-Entity Analytics.
/// These guide the AI to analyse aggregated data across multiple entities,
/// finding patterns, outliers, and strategic insights.
/// </summary>
public static class AnalyticsPromptTemplates
{
    // ─────────────────────────────────────────────
    //  SYSTEM PROMPT — strategic analyst persona
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 System prompt for cross-entity analytics.
    /// Different from the single-entity AnalyseSystem — this persona thinks
    /// at the portfolio/operational level, not the individual record level.
    /// It looks for patterns, outliers, concentrations, and trends across
    /// the whole dataset.
    /// </summary>
    public const string AnalyticsSystem = """
        You are a senior strategic analyst embedded in ApparelPro, a garment and apparel
        manufacturing ERP system. You analyse data ACROSS multiple entities to surface
        patterns, outliers, and strategic insights that are invisible when looking at
        individual records.

        You have expert knowledge of apparel manufacturing operations: costing (FOB, CMT, CIF),
        bill of materials (BOM), material consumption planning, wastage control, fabric yield,
        trim procurement, supplier management, and production scheduling.

        Your role is to think at the PORTFOLIO level — not individual styles or orders,
        but across the business: which buyers drive the most revenue, which suppliers
        are over-relied on, where wastage is highest, which orders are at risk.

        Your analysis must follow this structure:

        1. **EXECUTIVE SUMMARY** — 2-3 sentences capturing the single most important
           insight from this data. Lead with the number that matters most.

        2. **KEY METRICS** — The critical numbers calculated from the aggregated data.
           Show totals, averages, and distributions. Use tables for comparisons.

        3. **PATTERNS & TRENDS** — What the data reveals when viewed as a whole.
           Group findings by theme. Each finding states: the pattern, its magnitude,
           and why it matters for the business.

        4. **OUTLIERS & RISKS** — Flag anomalies that stand out from the norm.
           Rate each as 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH risk.
           Be specific: name the style, supplier, or order that's the outlier.

        5. **STRATEGIC RECOMMENDATIONS** — 3-5 actions ranked by business impact.
           Each states WHAT to do, WHY it matters, and the expected IMPACT.

        Formatting rules:
        - Use tables for rankings and comparisons (top 5, bottom 5, etc.).
        - Format currency to 2 decimal places with symbol.
        - Format percentages to 1 decimal place.
        - Use garment industry terminology throughout.
        - Never fabricate data. Only derive calculations from provided data.
        - If data is insufficient for a dimension, say so — don't pad.
        """;

    // ─────────────────────────────────────────────
    //  CATEGORY-SPECIFIC TEMPLATES
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 MATERIAL CONSUMPTION analytics — analyses BOM/consumption data across all styles.
    /// The service layer aggregates: total consumption per style, wastage allowances,
    /// supplier coverage, material cost drivers, and consumption density (lines per style).
    /// Placeholders: {0} = aggregated consumption JSON, {1} = optional user query.
    /// </summary>
    public const string MaterialConsumption = """
        Analyse material consumption patterns across all active styles in the system.

        The JSON below contains aggregated data for each style, including:
        • styleCode, order, buyerCode, buyerName — style identity
        • orderQuantity, unitPrice (FOB) — order economics
        • totalBomLines — number of BOM/consumption rows
        • directMaterialLines — BOM lines that are actual materials (not additional costs)
        • additionalCostLines — overhead/service lines in the BOM
        • totalPlannedConsumption — sum of totalConsumption across direct materials
        • averageWastagePercent — mean percentageAllowance across direct materials
        • maxWastagePercent — highest single wastage allowance in this style's BOM
        • supplierCoverage — percentage of direct materials with a supplier assigned
        • topMaterialByConsumption — the material with highest totalConsumption
        • estimatedMaterialCost — totalPlannedConsumption contextual indicator

        Analyse these dimensions:

        **A. Consumption Volume Distribution**
        - Rank styles by totalPlannedConsumption (highest to lowest).
        - Which styles are the biggest material consumers? Top 5.
        - What's the spread? (min, max, average, median if enough data.)

        **B. Wastage Analysis**
        - Average wastage across all styles vs industry benchmark (3-5% fabric, 2-3% trim).
        - Which styles have the highest wastage? Flag any > 8%.
        - Is wastage correlated with order size or garment type?

        **C. Supplier Coverage Gaps**
        - How many styles have < 100% supplier coverage on direct materials?
        - Which styles have the lowest supplier coverage? These are procurement risks.
        - Are specific material types commonly unassigned?

        **D. BOM Complexity**
        - Average BOM lines per style. Which styles are most complex?
        - Ratio of direct materials to additional costs — healthy range is 3:1 to 5:1.

        **E. Cost Concentration**
        - Which styles drive the highest estimated material cost?
        - Is there revenue concentration (few styles = most of the material spend)?

        Aggregated Consumption Data:
        {0}

        {1}
        """;

    /// <summary>
    /// 🎓 SUPPLIER PERFORMANCE analytics — analyses supplier distribution and coverage.
    /// The service layer aggregates: how many styles each supplier serves, material
    /// categories supplied, geographic coverage, contact completeness.
    /// Placeholders: {0} = aggregated supplier JSON, {1} = optional user query.
    /// </summary>
    public const string SupplierPerformance = """
        Analyse supplier distribution and coverage across all active styles and materials.

        The JSON below contains aggregated data for each supplier, including:
        • supplierCode, name — supplier identity
        • contactChannels — count of available contact methods (phone, mobile, fax)
        • hasAddress — whether a physical address is on file
        • stylesSupplied — number of distinct styles this supplier provides materials for
        • materialsSupplied — list of distinct material descriptions supplied
        • materialCategoryCount — number of distinct stock categories
        • totalConsumptionVolume — total material consumption across all styles served

        Also includes:
        • unassignedMaterials — materials across all styles with no supplier assigned
        • supplierConcentration — what percentage of total consumption the top 3 suppliers cover

        Analyse these dimensions:

        **A. Supplier Concentration Risk**
        - How much of total consumption depends on the top 1, 3, and 5 suppliers?
        - Flag if any single supplier covers > 30% of total consumption — single-point-of-failure risk.
        - Recommend diversification where concentration is high.

        **B. Supplier Coverage Gaps**
        - How many material lines have no supplier assigned?
        - Group unassigned materials by stock category — which categories are worst?
        - These represent procurement blind spots.

        **C. Supplier Breadth**
        - Which suppliers serve the most styles? Are they over-stretched?
        - Which suppliers specialise (1-2 categories) vs generalise (5+ categories)?

        **D. Contact & Communication Risk**
        - Flag suppliers with zero contact channels — unreachable during critical windows.
        - Flag suppliers with no address — can't verify or audit.

        **E. Strategic Recommendations**
        - Supplier development priorities (strengthen key suppliers).
        - Diversification actions (reduce single-supplier dependency).
        - Data completeness actions (fill contact/address gaps).

        Aggregated Supplier Data:
        {0}

        {1}
        """;

    /// <summary>
    /// 🎓 BUYER PORTFOLIO analytics — analyses buyer order volumes and revenue distribution.
    /// The service layer aggregates: order count per buyer, style count, total quantities,
    /// total estimated revenue, active vs inactive status.
    /// Placeholders: {0} = aggregated buyer JSON, {1} = optional user query.
    /// </summary>
    public const string BuyerPortfolio = """
        Analyse the buyer portfolio — order volumes, revenue distribution, and customer
        concentration across all buyers in the system.

        The JSON below contains aggregated data for each buyer, including:
        • buyerCode, buyerName, status (Active/Inactive) — buyer identity
        • totalOrders — number of purchase orders from this buyer
        • totalStyles — number of distinct styles for this buyer
        • totalOrderQuantity — sum of all order quantities (units/pieces)
        • totalEstimatedRevenue — sum of (quantity × unitPrice) across all styles
        • averageFobPrice — average FOB price across this buyer's styles
        • contactCompleteness — how many contact channels are filled
        • hasDeliveryAddress — whether a delivery address exists

        Analyse these dimensions:

        **A. Revenue Concentration**
        - Rank buyers by totalEstimatedRevenue (top to bottom).
        - What percentage of total revenue comes from the top 1, 3, and 5 buyers?
        - Flag if > 50% of revenue comes from a single buyer — business continuity risk.

        **B. Order Activity**
        - Which buyers are most active (most orders, most styles)?
        - Which buyers have high order counts but low revenue (small orders)?
        - Which buyers have few orders but high revenue (large strategic accounts)?

        **C. Pricing Analysis**
        - Compare averageFobPrice across buyers — who pays the highest/lowest FOB?
        - Are there buyers where FOB seems unusually low? (Margin pressure risk.)
        - Industry context: typical garment FOB ranges $3-50/unit depending on complexity.

        **D. Buyer Health**
        - Flag inactive buyers who still have open orders or styles.
        - Flag active buyers with missing contact info or no delivery address.
        - These data gaps block order processing and shipping.

        **E. Strategic Recommendations**
        - Customer diversification priorities.
        - Pricing review recommendations.
        - Data completeness actions for key accounts.

        Aggregated Buyer Data:
        {0}

        {1}
        """;

    /// <summary>
    /// 🎓 ORDER PIPELINE analytics — analyses purchase orders across the business.
    /// The service layer aggregates: order values, quantities, statuses, timelines,
    /// seasonal distribution, pricing basis breakdown.
    /// Placeholders: {0} = aggregated order JSON, {1} = optional user query.
    /// </summary>
    public const string OrderPipeline = """
        Analyse the purchase order pipeline across the entire business.

        The JSON below contains aggregated data for each purchase order, including:
        • buyerCode, buyerName, order — order identity
        • garmentTypeName — garment category
        • totalQuantity — ordered quantity
        • currencyCode, basisCode (FOB/CIF/CMT), basisValue — pricing
        • orderValue — totalQuantity × basisValue
        • season — seasonal label
        • orderDate — when the order was placed
        • daysSinceOrder — elapsed days since order date
        • hasDescription — whether the order has a description
        • quantityOverridden — whether quantity was manually changed

        Analyse these dimensions:

        **A. Pipeline Value**
        - Total pipeline value (sum of all order values) by currency.
        - Average order value. Distribution: how many small (<$10K), medium ($10K-$50K),
          large (>$50K) orders?

        **B. Buyer Distribution**
        - Orders by buyer — who has the most orders in the pipeline?
        - Value by buyer — who drives the most pipeline value?

        **C. Garment Mix**
        - Orders by garment type — what's the product mix?
        - Value by garment type — which categories are highest value?

        **D. Pricing Analysis**
        - Breakdown by pricing basis (FOB vs CIF vs CMT).
        - Average basisValue by garment type — are prices consistent?
        - Flag outlier prices (unusually high or low for their garment type).

        **E. Timeline & Data Quality**
        - Age distribution of orders (daysSinceOrder). Flag very old orders (>180 days).
        - Orders with missing descriptions. Orders with overridden quantities.

        Aggregated Order Data:
        {0}

        {1}
        """;

    /// <summary>
    /// 🎓 COST ANALYSIS analytics — deep dive into cost structures across styles.
    /// Analyses FOB pricing, material costs, wastage impact on cost, and margin indicators.
    /// Placeholders: {0} = aggregated cost JSON, {1} = optional user query.
    /// </summary>
    public const string CostAnalysis = """
        Analyse cost structures across all active styles to identify pricing patterns,
        cost drivers, and margin indicators.

        The JSON below contains cost data for each style, including:
        • styleCode, order, buyerCode, buyerName — style identity
        • garmentTypeName — garment category
        • orderQuantity, unitPrice (FOB per unit) — order economics
        • totalOrderValue — orderQuantity × unitPrice
        • totalPlannedConsumption — sum of material consumption quantities
        • directMaterialLineCount — number of material BOM lines
        • additionalCostLineCount — number of overhead BOM lines
        • averageWastage — mean wastage % across direct materials
        • wastageImpact — estimated consumption added by wastage allowances
        • topCostDriverMaterial — material with highest consumption

        Analyse these dimensions:

        **A. Revenue Distribution**
        - Total revenue across all styles. Top 10 styles by totalOrderValue.
        - Revenue by buyer — which buyers drive the most value?
        - Revenue by garment type — which categories are most valuable?

        **B. Cost Structure Patterns**
        - Average direct material lines per style vs additional cost lines.
        - Styles with the highest totalPlannedConsumption relative to orderQuantity
          (consumption per garment). These may have margin pressure.
        - Identify cost-efficient styles (low consumption, high FOB).

        **C. Wastage Cost Impact**
        - Total wastageImpact across all styles (what wastage costs the business).
        - Styles where wastage impact is highest — targets for wastage reduction.
        - Estimated savings if wastage were reduced to industry benchmark (5%).

        **D. Pricing Consistency**
        - FOB price distribution by garment type. Are prices consistent within types?
        - Flag styles where FOB seems low relative to material consumption
          (potential margin squeeze).

        **E. Strategic Recommendations**
        - Cost reduction opportunities (top 3 by impact).
        - Pricing review candidates.
        - Wastage reduction targets.

        Aggregated Cost Data:
        {0}

        {1}
        """;

    /// <summary>
    /// 🎓 Resolves the correct analytics template for a given category.
    /// The category string comes from the API request and determines which
    /// data aggregation query runs AND which prompt template guides the AI.
    /// </summary>
    public static string GetAnalyticsTemplate(string category) =>
        category.ToUpperInvariant() switch
        {
            "MATERIAL_CONSUMPTION" => MaterialConsumption,
            "SUPPLIER_PERFORMANCE" => SupplierPerformance,
            "BUYER_PORTFOLIO" => BuyerPortfolio,
            "ORDER_PIPELINE" => OrderPipeline,
            "COST_ANALYSIS" => CostAnalysis,
            _ => throw new ArgumentException(
                $"Unsupported analytics category: '{category}'. " +
                "Supported: MATERIAL_CONSUMPTION, SUPPLIER_PERFORMANCE, " +
                "BUYER_PORTFOLIO, ORDER_PIPELINE, COST_ANALYSIS.")
        };
}
