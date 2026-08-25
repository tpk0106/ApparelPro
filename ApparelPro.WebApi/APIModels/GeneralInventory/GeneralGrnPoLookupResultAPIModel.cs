namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGrnPoLookupResultAPIModel
    {
        public string PoNumber { get; set; } = null!;
        public string SupplierCode { get; set; } = null!;
        public string? CurrencyCode { get; set; }
        public List<GeneralGrnReceivableLineAPIModel> Lines { get; set; } = new();
    }
}
