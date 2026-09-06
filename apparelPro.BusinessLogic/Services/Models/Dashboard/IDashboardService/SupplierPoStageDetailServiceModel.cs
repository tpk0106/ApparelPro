namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    // Stage 2 - StyleMaterialCostProfile.BalanceQuantity is already a rolling
    // "still needs a PO" figure per material line (it decreases as Supplier
    // POs get raised against it - see SupplierPurchaseOrderService's own
    // GetUnfulfilledBudgetLinesAsync, which filters on BalanceQuantity > 0
    // for exactly this purpose). RaisedQuantity/RaisedValue come from the
    // Supplier PO lines actually raised so far for this style.
    public class SupplierPoStageDetailServiceModel
    {
        public decimal RaisedQuantity { get; set; }
        public decimal RaisedValue { get; set; }
        public decimal OutstandingQuantity { get; set; }
        public decimal OutstandingValue { get; set; }
        public string Currency { get; set; } = "";
    }
}
