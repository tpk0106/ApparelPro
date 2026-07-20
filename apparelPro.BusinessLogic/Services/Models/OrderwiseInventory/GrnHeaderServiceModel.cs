namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GrnHeaderServiceModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Resolved server-side from the PurchaseOrderHeader row during commit — never
        // trusted from client input, even though the frontend will echo these back from
        // a prior GetReceivableLinesByPoAsync call.
        public string StoreCode { get; set; } = null!;
        public string SupplierCode { get; set; } = null!;
    }
}
