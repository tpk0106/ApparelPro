namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSanLineItemAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = null!;
    }
}
