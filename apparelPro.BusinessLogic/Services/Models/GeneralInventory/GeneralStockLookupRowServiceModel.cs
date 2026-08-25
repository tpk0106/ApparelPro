namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockLookupRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }
    }
}
