namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStrnPrintLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
