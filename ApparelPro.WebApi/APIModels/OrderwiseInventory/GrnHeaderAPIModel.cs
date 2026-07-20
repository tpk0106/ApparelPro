namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnHeaderAPIModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
        public string SupplierCode { get; set; } = null!;
    }
}
