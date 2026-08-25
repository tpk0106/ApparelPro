namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralPoPrintLineServiceModel
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
