namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StrnPrintHeaderServiceModel
    {
        public string StrnNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        // Added alongside the same Buyer-name fix already applied to the Stock
        // Movement Report — legacy IN_STRN2.PRG only ever printed the raw buyer
        // code, but the modernized print run reads better with the name.
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Legacy IN_STRN2.PRG prints the current system date/time on every print run
        // (via inv_head's c_tod(date)+time()), not the original transaction date — the
        // note can be reprinted on demand and always shows "now", not "when committed".
        public DateTime PrintedOn { get; set; }
    }
}
