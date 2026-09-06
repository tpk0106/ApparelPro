namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    // Stage 1 - backed by the real Trim Sheet approval gate (Style.Username /
    // Style.ApprovedDate), the same fields SupplierPurchaseOrderService
    // already checks before letting a Supplier PO be raised. There is no
    // stored "revision requested" reason anywhere in the schema, so this can
    // only distinguish approved vs. not-yet-approved, not why it's pending.
    public class ApprovalStageDetailServiceModel
    {
        public bool IsApproved { get; set; }
        public string? ApprovedBy { get; set; }
        public DateOnly? ApprovedDate { get; set; }
    }
}
