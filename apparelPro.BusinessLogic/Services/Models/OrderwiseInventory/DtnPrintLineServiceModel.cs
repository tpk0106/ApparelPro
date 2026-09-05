namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class DtnPrintLineServiceModel
    {
        // Legacy IN_DTN2.PRG only ever prints the From-side item code/description/unit/qty -
        // the destination item mapping isn't shown on the printed note, even though it's
        // tracked internally. Same convention kept here.
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
