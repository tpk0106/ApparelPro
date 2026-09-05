namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class RawMaterialControlSheetHeaderServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string ItemDescription { get; set; } = "";
        public decimal OrderQuantity { get; set; }
        public string Unit { get; set; } = "";
        public int TotalLineItems { get; set; }
    }
}
