namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    // Item picker row - shape differs slightly by Direction: for GeneralToOrder it's
    // sourced from GeneralStockMaster (store's own balance); for OrderToGeneral it's
    // sourced from OrderwiseStock (that Buyer/Order's balance at the store).
    public class OrderGtnTransferableStockRowServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal AvailableBalance { get; set; }
    }
}
