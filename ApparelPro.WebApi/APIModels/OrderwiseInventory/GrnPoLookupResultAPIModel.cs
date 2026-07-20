namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnPoLookupResultAPIModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string SupplierCode { get; set; } = null!;
        public List<GrnReceivableLineAPIModel> Lines { get; set; } = new();
    }
}
