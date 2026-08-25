namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGtnPrintLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
