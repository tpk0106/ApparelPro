namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGrnLineItemAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
