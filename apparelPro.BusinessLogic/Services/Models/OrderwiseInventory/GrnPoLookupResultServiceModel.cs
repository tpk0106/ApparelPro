namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GrnPoLookupResultServiceModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string SupplierCode { get; set; } = null!;
        public List<GrnReceivableLineServiceModel> Lines { get; set; } = new();
    }
}
