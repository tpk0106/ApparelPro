namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class SanPrintLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        // The physical count entered during the stock take - SAN SETS QtyInHand to this
        // value rather than adding/subtracting a movement (see StockAdjustmentNoteService).
        public decimal AdjustedQuantity { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
