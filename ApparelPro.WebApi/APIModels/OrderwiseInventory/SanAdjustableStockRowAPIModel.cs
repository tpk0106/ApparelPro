namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SanAdjustableStockRowAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;

        // Current physical count — informational context only, no ceiling. See
        // SanAdjustableStockRowServiceModel's comment for why there's no Max* field here.
        public decimal QtyInHand { get; set; }
    }
}
