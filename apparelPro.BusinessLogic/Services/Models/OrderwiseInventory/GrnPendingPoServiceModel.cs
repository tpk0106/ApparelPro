namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // One entry per outstanding PO for a given Buyer+Order — powers a "pick a PO from a
    // list" dropdown as an alternate entry point to typing the PO number directly.
    public class GrnPendingPoServiceModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public string SupplierCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
    }
}
