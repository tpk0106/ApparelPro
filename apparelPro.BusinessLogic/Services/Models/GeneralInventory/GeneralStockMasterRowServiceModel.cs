namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockMasterRowServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // full 22-char composite
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
        public decimal QtyInHand { get; set; }
    }
}
