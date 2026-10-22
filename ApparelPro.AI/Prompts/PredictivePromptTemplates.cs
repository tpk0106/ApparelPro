namespace ApparelPro.AI.Prompts;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 PREDICTIVE AI PROMPT TEMPLATES
// Phase 5: Predictive & Prescriptive AI — prompts that guide the AI to make
// FORWARD-LOOKING predictions, not backward-looking analysis.
//
// 🎓 KEY DIFFERENCE FROM AnalyticsPromptTemplates:
// Analytics prompts say: "Here's data — find patterns and outliers."
// Predictive prompts say: "Here's historical data + computed signals —
//   forecast what will happen and recommend what to do about it."
//
// 🎓 THE SHIFT FROM DESCRIPTIVE → PREDICTIVE → PRESCRIPTIVE:
//   Phase 2 (Analytics):   "Your wastage is 8.2% on average"          (descriptive)
//   Phase 5 (Predictive):  "Wastage will exceed 10% next quarter"     (predictive)
//   Phase 5 (Prescriptive): "Reduce wastage on Style X to save $4K"   (prescriptive)
//
// 🎓 PROMPT DESIGN PRINCIPLES FOR PREDICTIONS:
//   1. Always provide HISTORICAL CONTEXT — the AI needs time-series data
//      or derived trend signals (growth rates, velocity) to forecast.
//   2. Always include COMPUTED SIGNALS — raw data alone isn't enough.
//      We pre-compute things like "order velocity", "consumption runway",
//      "supplier concentration ratio" so the AI builds on QUANTIFIED signals.
//   3. Always ask for CONFIDENCE LEVELS — predictions aren't certain.
//      The AI must say "high/medium/low confidence" and explain why.
//   4. Always ask for ACTIONABLE RECOMMENDATIONS — predictions without
//      actions are interesting but useless. Every prediction must end
//      with "here's what you should do about it."
//   5. Always specify a TIME HORIZON — "next 3 months" is actionable,
//      "sometime in the future" is not.
//
// 🎓 PRESCRIPTIVE AI DESIGN PRINCIPLES (Phase 5 Part 2 Enhancement):
//   6. CAUSALITY CHAINS — every recommendation states "If X → expect Y"
//      so the user understands the mechanism behind the advice.
//   7. PRIORITY SCORING — P1/P2/P3/P4 ranking based on impact × urgency
//      × implementation ease, so the user knows what to tackle first.
//   8. DECISION MATRICES — for major decisions, present 2-3 options with
//      trade-offs (cost vs benefit vs risk vs time) so the user can choose.
//   9. QUANTIFIED IMPACT — every action includes $ or % estimates, not
//      vague "significant" or "considerable" language.
//  10. IMPLEMENTATION STEPS — who owns it, what's the first step, and
//      what does success look like (measurable metrics).
//
// 🎓 SUPPORTED PREDICTION CATEGORIES:
//   DEMAND_FORECASTING  — future order quantities from historical patterns
//   MATERIAL_SHORTAGE   — materials at risk based on consumption vs capacity
//   SUPPLIER_RISK       — risk scores from multiple weighted factors
//   COST_OPTIMIZATION   — specific cost reduction actions with estimated savings
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Prompt templates for Phase 5: Predictive &amp; Prescriptive AI.
/// These guide the AI to forecast future outcomes and recommend actions
/// based on historical data and computed trend signals.
/// </summary>
public static class PredictivePromptTemplates
{
    // ─────────────────────────────────────────────
    //  SYSTEM PROMPT — predictive analyst persona
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 System prompt for predictive AI.
    /// Different from the analytics AnalyticsSystem — this persona FORECASTS
    /// future outcomes rather than analysing current state. It thinks in terms
    /// of trends, velocities, risk probabilities, and actionable recommendations.
    ///
    /// The key instruction: "Every prediction must state its confidence level
    /// and the signals it's based on. Every prediction must end with specific
    /// actions the user should take."
    /// </summary>
    public const string PredictiveSystem = """
        You are a senior predictive analyst embedded in ApparelPro, a garment and apparel
        manufacturing ERP system. Your role is to FORECAST future outcomes and PRESCRIBE
        actions based on historical data and computed trend signals.

        You have expert knowledge of apparel manufacturing operations: demand planning,
        material requirements planning (MRP), supplier lead times, fabric yield forecasting,
        trim procurement cycles, costing structures (FOB, CMT, CIF), and seasonal
        production patterns.

        CRITICAL RULES FOR PREDICTIONS:
        1. Every forecast must state a CONFIDENCE LEVEL: 🟢 HIGH (strong signal, enough data),
           🟡 MEDIUM (moderate signal, some uncertainty), or 🔴 LOW (weak signal, limited data).
        2. Always cite the SPECIFIC SIGNALS your prediction is based on — never predict
           from nothing. Name the metric, its value, and its trend direction.
        3. Every prediction must end with ACTIONABLE RECOMMENDATIONS — ranked by
           urgency and expected impact. "What to do" + "By when" + "Expected result."
        4. Never fabricate data points. If historical data is too thin for a reliable
           forecast, say so and provide a range estimate instead of a point prediction.
        5. Use time horizons explicitly: "In the next 30 days", "Within 3 months",
           "By end of quarter." Vague timeframes are useless for planning.

        CRITICAL RULES FOR PRESCRIPTIVE RECOMMENDATIONS:
        6. Every recommendation must include a CAUSALITY CHAIN: "If you do X → expect Y
           within Z timeframe." Never give an action without its expected consequence.
        7. Quantify the EXPECTED IMPACT of every action in dollars, percentages, or units.
           If exact numbers aren't possible, give a realistic range (e.g., "$2,000–$4,500").
        8. Assign a PRIORITY SCORE (P1 Critical / P2 High / P3 Medium / P4 Low) to every
           action based on: impact magnitude × urgency × implementation ease.
        9. For major decisions, present a DECISION MATRIX showing 2-3 options with their
           trade-offs (cost vs benefit vs risk vs time-to-implement) so the user can choose.
        10. Include IMPLEMENTATION STEPS for high-priority actions — not just "what" but
            "how": who should own it, what the first step is, and what success looks like.

        Your analysis must follow this structure:

        1. **PREDICTION SUMMARY** — 2-3 sentences with the headline forecast.
           Lead with the most impactful prediction and its confidence level.

        2. **DATA SIGNALS** — The computed indicators your predictions are based on.
           For each signal: its current value, trend direction (↑↗→↘↓), and significance.
           Use a table for clarity.

        3. **DETAILED FORECAST** — Category-specific predictions with numbers.
           Include best-case, expected, and worst-case scenarios where appropriate.
           Time-bound every prediction to the requested horizon.

        4. **RISK FACTORS** — What could make the prediction wrong (both directions).
           Rate each risk as 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH probability.

        5. **PRESCRIPTIVE ACTION PLAN** — This is the most important section.
           For each recommended action, provide ALL of the following:

           | Field | Content |
           |-------|---------|
           | Action | Clear, specific action statement |
           | Priority | P1 Critical / P2 High / P3 Medium / P4 Low |
           | Owner | Which role should execute (e.g., Merchandiser, Procurement, Production) |
           | Timeline | Specific deadline or timeframe |
           | Causality | "If we do X → Y happens because Z" |
           | Expected Impact | Quantified in $ or % or units |
           | First Step | The immediate next action to start |
           | Success Metric | How to measure if the action worked |

           Group actions into tiers:
           - 🔴 IMMEDIATE (this week): crisis prevention, zero-cost process fixes
           - 🟡 SHORT-TERM (this month): negotiations, sourcing changes, BOM adjustments
           - 🟢 STRATEGIC (this quarter): supplier development, process re-engineering

        6. **DECISION MATRIX** (when applicable) — For the top 1-2 decisions where
           multiple valid approaches exist, present a comparison table:

           | Option | Cost | Benefit | Risk | Time | Recommendation |
           |--------|------|---------|------|------|----------------|

           State which option you recommend and WHY, but present all options fairly
           so the decision-maker can choose based on their priorities.

        Formatting rules:
        - Use tables for rankings, comparisons, signal summaries, and action plans.
        - Format currency to 2 decimal places with symbol.
        - Format percentages to 1 decimal place.
        - Use garment industry terminology throughout.
        - Distinguish clearly between FACTS (from data) and PREDICTIONS (from signals).
        - Never present a prediction as a fact.
        - Bold the most critical numbers and recommendations for scannability.
        """;

    // ─────────────────────────────────────────────
    //  CATEGORY-SPECIFIC TEMPLATES
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 DEMAND FORECASTING — predicts future order quantities.
    ///
    /// DATA STRATEGY:
    /// The service aggregates per-buyer ordering patterns:
    ///   - Historical order counts and quantities per time period
    ///   - Average order size and order frequency (velocity)
    ///   - Seasonal patterns (if season field is populated)
    ///   - Recent trend direction (last 3 months vs prior 3 months)
    ///   - Style-level demand concentration (which styles drive volume)
    ///
    /// The AI uses these signals to forecast:
    ///   - Expected order volume for the next N months by buyer
    ///   - Which buyers are growing vs declining
    ///   - Seasonal peaks to prepare for
    ///   - Revenue forecast implications
    ///
    /// Placeholders: {0} = aggregated demand JSON, {1} = time horizon months,
    ///               {2} = optional user query
    /// </summary>
    public const string DemandForecasting = """
        Forecast future order demand for the next {1} months based on historical
        ordering patterns across all buyers and styles.

        The JSON below contains aggregated demand signals for each buyer, including:

        PER-BUYER SIGNALS:
        • buyerCode, buyerName — buyer identity
        • totalOrders — total number of purchase orders (all time)
        • totalStyles — number of distinct styles ordered
        • totalOrderQuantity — sum of all order quantities (units)
        • totalEstimatedRevenue — sum of (quantity × unitPrice) across styles
        • averageOrderQuantity — mean order quantity per style
        • averageFobPrice — mean FOB price per unit
        • earliestOrderDate, latestOrderDate — order history span
        • orderSpanDays — days between first and last order (history depth)
        • orderVelocity — orders per month (totalOrders / orderSpanMonths)
        • quantityVelocity — units per month (totalOrderQuantity / orderSpanMonths)
        • revenueVelocity — estimated revenue per month
        • recentOrderCount — orders in the last 90 days
        • recentQuantity — units ordered in the last 90 days
        • recentVsPriorRatio — ratio of last-90-days to prior-90-days quantity
          (> 1.0 = growing, < 1.0 = declining, null = insufficient history)
        • topStylesByQuantity — top 3 styles by order quantity

        BUSINESS-WIDE SIGNALS:
        • totalBuyerCount — number of active buyers
        • businessWideOrderVelocity — total orders per month across all buyers
        • businessWideQuantityVelocity — total units per month
        • businessWideRevenueVelocity — total revenue per month

        Forecast these dimensions for the next {1} months:

        **A. Buyer-Level Demand Forecast**
        - For each significant buyer (top 10 by revenue): predicted order quantity
          for the next {1} months, with confidence level.
        - Flag buyers whose recentVsPriorRatio indicates acceleration (> 1.2) or
          deceleration (< 0.8).
        - Buyers with no recent orders but historical volume — are they churning?

        **B. Business-Wide Volume Forecast**
        - Total predicted order quantity and estimated revenue for the next {1} months.
        - Best-case / Expected / Worst-case scenarios based on signal strength.

        **C. Seasonal & Trend Patterns**
        - Are there visible seasonal patterns in the order data?
        - Is the overall business trending up, flat, or down?
        - Which garment types are gaining vs losing demand?

        **D. Capacity & Resource Implications**
        - Based on the forecast, estimate production capacity needs.
        - Flag if predicted demand exceeds recent run rates by > 20% (capacity risk).
        - Material procurement lead time considerations.

        **E. Prescriptive Action Plan**
        For each action, provide a structured recommendation:

        🔴 IMMEDIATE ACTIONS (this week):
        - For each: Action | Owner (Merchandiser/Production/Procurement) | Causality
          ("If we do X → expect Y because Z") | Expected Impact ($, %, or units)

        🟡 SHORT-TERM ACTIONS (within {1} months):
        - Buyer relationship priorities: grow accelerating buyers, re-engage decelerating ones.
        - For each buyer action: specific outreach strategy, target order increase, revenue impact.

        🟢 STRATEGIC ACTIONS (next 2 quarters):
        - Capacity planning, new buyer acquisition, product diversification.
        - For each: implementation steps, investment required, expected ROI.

        **F. Decision Matrix** (if demand is shifting significantly):
        Present 2-3 strategic options (e.g., "invest in capacity" vs "selective growth" vs
        "consolidate") with trade-offs:
        | Option | Investment | Revenue Upside | Risk | Timeline | Recommendation |

        Aggregated Demand Data:
        {0}

        {2}
        """;

    /// <summary>
    /// 🎓 MATERIAL SHORTAGE PREDICTOR — identifies materials at risk of shortage.
    ///
    /// DATA STRATEGY:
    /// The service computes per-material signals:
    ///   - Total planned consumption across all active styles
    ///   - Number of styles depending on this material
    ///   - Supplier coverage (how many suppliers can provide it)
    ///   - Whether the material is sole-sourced (single supplier = high risk)
    ///   - Consumption concentration (is one style consuming 80% of it?)
    ///
    /// The AI uses these signals to predict:
    ///   - Which materials are most likely to face shortage
    ///   - Root cause: sole-source risk, high demand concentration, or no supplier
    ///   - Urgency: immediate (< 30 days), near-term (30-90), medium-term (90+)
    ///
    /// Placeholders: {0} = aggregated material JSON, {1} = time horizon months,
    ///               {2} = optional user query
    /// </summary>
    public const string MaterialShortage = """
        Predict which materials are at risk of shortage within the next {1} months
        based on planned consumption, supplier coverage, and demand concentration.

        The JSON below contains shortage risk signals for each material, including:

        PER-MATERIAL SIGNALS:
        • stockCode, itemCode — material identity
        • materialDescription — human-readable name (stockCode/itemCode combined)
        • totalPlannedConsumption — total consumption across all styles needing this material
        • stylesDependingOnMaterial — how many styles use this material
        • styleDependencyList — list of (styleCode, order, buyerCode) tuples depending on it
        • supplierCount — number of distinct suppliers assigned to this material
        • supplierCodes — list of assigned supplier codes
        • isSoleSourced — true if only 1 supplier (critical SPOF risk)
        • isUnsourced — true if NO supplier assigned (immediate risk)
        • consumptionConcentration — percentage of total consumption from the top style
          (> 70% = high concentration risk — one style's cancellation could idle supply)
        • averageWastagePercent — mean wastage allowance across styles using this material
        • maxWastagePercent — highest wastage on any style using this material

        AGGREGATE SIGNALS:
        • totalMaterialCount — total distinct materials in the system
        • unsourcedMaterialCount — materials with no supplier
        • soleSourcedMaterialCount — materials with only 1 supplier
        • highConcentrationMaterialCount — materials where one style consumes > 70%

        Predict shortage risk for each material:

        **A. Critical Shortage Risks (Immediate Action Required)**
        - Unsourced materials with high planned consumption — these WILL cause delays.
        - Sole-sourced materials where the single supplier serves many other styles too.
        - Materials with high consumption + high wastage (amplifies shortage risk).

        **B. Elevated Shortage Risks (Monitor Closely)**
        - Materials with high consumption concentration in one style or buyer.
        - Materials where supplier count is low (2-3) relative to demand.
        - Materials with above-average wastage (wastage accelerates consumption).

        **C. Shortage Timeline Predictions**
        - For each high-risk material: estimated time before shortage becomes critical.
        - Factor in typical procurement lead times (fabric: 4-8 weeks, trims: 2-4 weeks).
        - Flag materials where lead time > time to shortage.

        **D. Supply Chain Vulnerability Map**
        - Which buyers/orders would be most affected by each predicted shortage?
        - Cascade impact: if Material X runs short, which styles stop?
        - Revenue at risk from each shortage scenario.

        **E. Prescriptive Action Plan**
        For each action, provide a structured recommendation:

        🔴 IMMEDIATE ACTIONS (this week):
        - For every unsourced material: specific sourcing action, who to contact,
          expected lead time to resolve, and revenue at risk if not actioned.
        - Causality chain: "If Material X remains unsourced → Style Y delays by Z weeks
          → $W revenue at risk for Buyer B."

        🟡 SHORT-TERM ACTIONS (within {1} months):
        - Backup supplier development for sole-sourced materials.
        - For each: target supplier count, negotiation approach, expected timeline.
        - Wastage reduction targets: which materials, current %, target %, $ saving.

        🟢 STRATEGIC ACTIONS (next 2 quarters):
        - Supply base diversification roadmap with milestones.
        - Safety stock policy recommendations for critical materials.
        - For each: investment cost vs risk reduction benefit.

        **F. Decision Matrix** (for critical shortage scenarios):
        For the highest-risk material, compare resolution options:
        | Option | Cost | Lead Time | Risk Reduction | Trade-off | Recommendation |
        e.g., "Emergency spot buy" vs "Expedite existing supplier" vs "Substitute material"

        Aggregated Material Shortage Data:
        {0}

        {2}
        """;

    /// <summary>
    /// 🎓 SUPPLIER RISK SCORING — scores each supplier on multiple risk factors.
    ///
    /// DATA STRATEGY:
    /// The service computes a multi-dimensional risk profile for each supplier:
    ///   - Dependency score: how many styles/materials rely on this supplier
    ///   - Concentration score: what % of total consumption this supplier covers
    ///   - Contact risk: completeness of contact information
    ///   - Breadth risk: single-category vs multi-category supplier
    ///   - Revenue exposure: how much revenue depends on this supplier's materials
    ///
    /// The AI uses these signals to:
    ///   - Rank suppliers by overall risk score
    ///   - Identify which risk dimension is most concerning for each
    ///   - Recommend risk mitigation actions per supplier
    ///
    /// Placeholders: {0} = aggregated supplier risk JSON, {1} = time horizon months,
    ///               {2} = optional user query
    /// </summary>
    public const string SupplierRisk = """
        Score and rank suppliers by risk level, predicting which suppliers pose the
        greatest threat to operations over the next {1} months.

        The JSON below contains risk signals for each supplier, including:

        PER-SUPPLIER RISK SIGNALS:
        • supplierCode, name — supplier identity
        • contactChannels — count of contact methods (phone, mobile, fax): 0=unreachable
        • hasAddress — whether physical address is on file
        • stylesDependent — number of styles relying on this supplier
        • materialsSupplied — list of distinct materials provided
        • materialCategoryCount — number of distinct stock categories (breadth)
        • totalConsumptionVolume — total consumption across all styles served
        • consumptionSharePercent — this supplier's share of TOTAL business consumption
        • isCriticalSupplier — true if consumptionSharePercent > 15%
        • dependentBuyerCount — number of distinct buyers whose styles use this supplier
        • revenueExposure — estimated revenue dependent on this supplier's materials
        • soleSourcingCount — how many materials this supplier is the ONLY source for
        • riskFactors — pre-computed risk dimension flags:
          - concentrationRisk: HIGH if consumptionSharePercent > 20%
          - contactRisk: HIGH if contactChannels == 0
          - breadthRisk: HIGH if materialCategoryCount == 1 (single-product supplier)
          - dependencyRisk: HIGH if soleSourcingCount > 3
          - revenueRisk: HIGH if revenueExposure > 20% of total business revenue

        AGGREGATE SIGNALS:
        • totalSupplierCount — total active suppliers
        • criticalSupplierCount — suppliers with > 15% consumption share
        • unreachableSupplierCount — suppliers with 0 contact channels
        • highRiskSupplierCount — suppliers with 2+ HIGH risk dimensions

        Score and analyse supplier risk:

        **A. Supplier Risk Rankings**
        - Rank all suppliers from highest to lowest overall risk.
        - For each supplier in the top 10: name the dominant risk dimension
          and quantify the exposure.
        - Use a composite risk score: weight concentration (30%), dependency (25%),
          sole-sourcing (25%), contact (10%), breadth (10%).

        **B. Critical Supplier Deep Dive**
        - For each critical supplier (consumptionSharePercent > 15%):
          detail what would happen if they failed to deliver.
        - Estimate business impact: affected styles, affected buyers, revenue at risk.
        - How long could the business sustain without this supplier?

        **C. Systemic Risks**
        - What % of total consumption is covered by the top 3 suppliers?
        - Are there material categories with only 1-2 suppliers total?
        - Geographic concentration risk (if address data allows).

        **D. Risk Trend Prediction**
        - Based on current signals, which suppliers' risk is INCREASING?
        - Flag any supplier whose sole-sourcing count is growing.
        - Predict which risk events are most likely in the next {1} months.

        **E. Prescriptive Risk Mitigation Plan**
        For each action, provide a structured recommendation:

        🔴 IMMEDIATE ACTIONS (this week):
        - Contact completeness: list each unreachable supplier with action to fix.
          "If Supplier X remains unreachable → we cannot escalate delivery issues
          → $Y revenue exposed to unmanaged risk."
        - For each: Owner (Procurement), first step, success metric.

        🟡 SHORT-TERM ACTIONS (within {1} months):
        - Alternative source development for sole-sourced materials.
        - For each supplier: target backup count, qualification timeline,
          expected cost impact of dual-sourcing, risk reduction %.
        - Causality: "If we add Supplier B for Material M → sole-sourcing risk
          drops from HIGH to LOW → $Z revenue protected."

        🟢 STRATEGIC ACTIONS (next 2 quarters):
        - Supplier consolidation or diversification programme.
        - Vendor scorecard implementation: what to measure, frequency, thresholds.
        - For each: investment, ROI timeline, measurable risk reduction target.

        **F. Decision Matrix** (for critical supplier dependencies):
        For each critical supplier (>15% consumption), compare mitigation strategies:
        | Strategy | Cost | Time to Implement | Risk Reduction | Trade-off | Recommendation |
        e.g., "Develop 2nd source" vs "Negotiate long-term contract" vs "Vertical integration"

        Aggregated Supplier Risk Data:
        {0}

        {2}
        """;

    /// <summary>
    /// 🎓 COST OPTIMIZATION ADVISOR — recommends specific cost reduction actions.
    ///
    /// DATA STRATEGY:
    /// The service computes cost-saving opportunity signals:
    ///   - Wastage hotspots: styles with wastage > industry benchmark
    ///   - Pricing inconsistencies: same garment type with wide FOB variance
    ///   - BOM complexity outliers: styles with unusually many BOM lines
    ///   - Material substitution candidates: high-cost materials with alternatives
    ///   - Volume consolidation opportunities: same material across many styles
    ///
    /// The AI uses these signals to:
    ///   - Quantify potential savings for each opportunity
    ///   - Rank opportunities by estimated $ impact
    ///   - Provide specific, actionable recommendations
    ///
    /// Placeholders: {0} = aggregated cost optimization JSON, {1} = time horizon months,
    ///               {2} = optional user query
    /// </summary>
    public const string CostOptimization = """
        Identify and quantify cost optimisation opportunities across all active styles,
        recommending specific actions achievable within the next {1} months.

        The JSON below contains cost optimisation signals, including:

        PER-STYLE COST SIGNALS:
        • styleCode, order, buyerCode, buyerName — style identity
        • garmentTypeName — garment category
        • orderQuantity, unitPrice (FOB) — order economics
        • totalOrderValue — orderQuantity × unitPrice
        • totalPlannedConsumption — total material consumption
        • consumptionPerUnit — totalPlannedConsumption / orderQuantity
          (material intensity — lower is more efficient)
        • directMaterialLineCount, additionalCostLineCount — BOM complexity
        • averageWastage — mean wastage % across direct materials
        • wastageImpact — estimated excess consumption from wastage
        • wastageImpactCost — estimated $ cost of wastage (if unitPrice available)
        • potentialWastageSaving — $ saved if wastage reduced to 5% benchmark
        • topCostDriverMaterial — material with highest consumption
        • fobPerGarmentType — average FOB for this garment type across all styles
        • fobVarianceFromTypeAvg — how far this style's FOB deviates from type average
          (negative = below average price, potential margin squeeze)

        AGGREGATE OPTIMIZATION SIGNALS:
        • totalWastageImpactCost — business-wide $ cost of wastage
        • totalPotentialSaving — business-wide $ saving if all wastage → 5%
        • bomComplexityOutlierCount — styles with BOM lines > 2× average
        • pricingInconsistencyCount — garment types with FOB coefficient of variation > 30%
        • topWastageStyles — 10 styles with highest wastageImpactCost
        • topConsumptionStyles — 10 styles consuming most material per unit

        Identify and quantify cost optimisation opportunities:

        **A. Wastage Reduction Opportunities**
        - Rank styles by wastageImpactCost (highest $ impact first).
        - Top 10 wastage offenders: style, current wastage %, estimated $ impact,
          target wastage, estimated $ saving.
        - Total business-wide saving if wastage benchmark achieved.
        - Which material types have the worst wastage? (fabric vs trim)

        **B. Pricing Optimisation**
        - Garment types with highest FOB variance — are we under-pricing?
        - Styles where FOB is significantly below type average — margin risk.
        - Styles where FOB is above average — can we leverage for more orders?
        - Recommended pricing adjustments with estimated revenue impact.

        **C. BOM Complexity Reduction**
        - Styles with unusually high BOM line counts — can materials be consolidated?
        - High additional-cost-to-material ratios — overhead creep indicators.
        - Material rationalisation: same material appearing in many styles that
          could be volume-consolidated for better pricing.

        **D. Material Efficiency**
        - Styles with highest consumptionPerUnit — material-intensive production.
        - Compare consumptionPerUnit within the same garment type — outliers
          indicate inefficient cutting or over-specification.
        - Material substitution candidates: high-consumption materials where
          lower-consumption alternatives exist in other styles.

        **E. Prescriptive Savings Roadmap**
        Rank ALL identified savings opportunities and present as a structured action plan.
        For each action, provide:

        🔴 IMMEDIATE ACTIONS (this week) — zero-cost process fixes:
        - For each: specific style/material to target, current metric, target metric,
          $ saving, who owns it, first step.
        - Causality: "If we reduce wastage on Style X from 12% to 5% → save $Y/month
          because Z fewer units of material are consumed per garment."

        🟡 SHORT-TERM ACTIONS (within {1} months) — negotiations + BOM changes:
        - Pricing renegotiations: which buyers/styles, current vs target FOB, revenue impact.
        - BOM simplification: which styles have excessive line counts, what to consolidate.
        - For each: expected $ impact, effort level (Low/Medium/High), timeline.

        🟢 STRATEGIC ACTIONS (next 2 quarters) — structural changes:
        - Material substitution candidates with cost comparison.
        - Volume consolidation opportunities across styles.
        - Supplier pricing leverage from consolidated purchasing.
        - For each: investment required, payback period, annual $ saving.

        **Total Savings Summary Table:**
        | Tier | Action Count | Total Potential Saving | Avg. Implementation Time |
        |------|-------------|----------------------|-------------------------|

        **F. Decision Matrix** (for top savings opportunity):
        Compare implementation approaches for the single highest-$ opportunity:
        | Approach | Upfront Cost | Monthly Saving | Risk | Disruption | Recommendation |
        e.g., "Immediate wastage target" vs "Renegotiate material pricing" vs "Redesign BOM"

        Aggregated Cost Optimisation Data:
        {0}

        {2}
        """;

    /// <summary>
    /// 🎓 Resolves the correct prediction template for a given category.
    /// The category string comes from the API request and determines which
    /// data aggregation query runs AND which prompt template guides the AI.
    ///
    /// Note: Unlike AnalyticsPromptTemplates.GetAnalyticsTemplate which returns
    /// templates with {0} and {1} placeholders, predictive templates use
    /// {0} = data JSON, {1} = time horizon months, {2} = optional user query.
    /// </summary>
    public static string GetPredictiveTemplate(string category) =>
        category.ToUpperInvariant() switch
        {
            "DEMAND_FORECASTING" => DemandForecasting,
            "MATERIAL_SHORTAGE"  => MaterialShortage,
            "SUPPLIER_RISK"      => SupplierRisk,
            "COST_OPTIMIZATION"  => CostOptimization,
            _ => throw new ArgumentException(
                $"Unsupported prediction category: '{category}'. " +
                "Supported: DEMAND_FORECASTING, MATERIAL_SHORTAGE, " +
                "SUPPLIER_RISK, COST_OPTIMIZATION.")
        };
}
