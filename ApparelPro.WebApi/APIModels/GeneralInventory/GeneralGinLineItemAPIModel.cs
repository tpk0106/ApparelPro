namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGinLineItemAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
