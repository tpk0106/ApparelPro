namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSanPrintLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
