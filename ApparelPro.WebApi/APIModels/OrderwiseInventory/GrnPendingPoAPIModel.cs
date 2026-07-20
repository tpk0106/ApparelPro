namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnPendingPoAPIModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public string SupplierCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
    }
}
