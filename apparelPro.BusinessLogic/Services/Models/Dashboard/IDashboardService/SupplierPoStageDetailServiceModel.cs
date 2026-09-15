namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    // Stage 2 - StyleMaterialCostProfile.BalanceQuantity is already a rolling
    // "still needs a PO" figure per material line (it decreases as Supplier
    // POs get raised against it - see SupplierPurchaseOrderService's own
    // GetUnfulfilledBudgetLinesAsync, which filters on BalanceQuantity > 0
    // for exactly this purpose).
    //
    // REVISED 2026-09-15: RaisedQuantity/OutstandingQuantity used to be raw
    // sums of every material line's quantity - meaningless, since a style's
    // material lines mix units (GRS for buttons, YDS for fabric, PCS for
    // bags, ...) and summing them is like adding kilograms to litres. Those
    // two fields are gone. CoveragePercent replaces them: a value-weighted
    // ratio (committed value / total required value) which is unit-agnostic
    // by construction, so it's the one honest single number for "how much of
    // this style's material commitment is covered". Bottleneck surfaces the
    // single highest-financial-exposure outstanding line so the UI can lead
    // with the actual blocker instead of a wall of numbers; OutstandingLines
    // carries the full per-line breakdown for drill-down.
    public class SupplierPoStageDetailServiceModel
    {
        public decimal CoveragePercent { get; set; } // 0-100, value-weighted across every material line
        public decimal RaisedValue { get; set; }
        public decimal OutstandingValue { get; set; }
        public string Currency { get; set; } = "";
        public MaterialLineServiceModel? Bottleneck { get; set; } // null when fully covered
        public List<MaterialLineServiceModel> OutstandingLines { get; set; } = new();
    }

    public class MaterialLineServiceModel
    {
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal RequiredQuantity { get; set; }
        public decimal RaisedQuantity { get; set; }
        public decimal OutstandingQuantity { get; set; }
        public decimal OutstandingValue { get; set; }
        public decimal CoveredPercent { get; set; }
    }
}
