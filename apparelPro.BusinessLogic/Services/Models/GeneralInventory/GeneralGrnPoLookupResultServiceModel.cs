namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGrnPoLookupResultServiceModel
    {
        public string PoNumber { get; set; } = null!;
        public string SupplierCode { get; set; } = null!;
        public string? CurrencyCode { get; set; }
        public List<GeneralGrnReceivableLineServiceModel> Lines { get; set; } = new();
    }
}
