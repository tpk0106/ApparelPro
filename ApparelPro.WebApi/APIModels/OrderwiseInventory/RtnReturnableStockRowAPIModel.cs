namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class RtnReturnableStockRowAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }

        // Hard ceiling for the Quantity field on the entry grid — legacy: "Return
        // Quantity cannot be greater than Total issues."
        public decimal MaxReturnableQuantity { get; set; }
    }
}
