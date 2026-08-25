namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralRtnLineItemAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
