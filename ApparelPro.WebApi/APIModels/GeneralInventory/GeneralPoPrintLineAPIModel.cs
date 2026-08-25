namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralPoPrintLineAPIModel
    {
        public string RefNo { get; set; } = "";
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public decimal Price { get; set; }
        public DateTime? ExpectedDate { get; set; }
    }
}
