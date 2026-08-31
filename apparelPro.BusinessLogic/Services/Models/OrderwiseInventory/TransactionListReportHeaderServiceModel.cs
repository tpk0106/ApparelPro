namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class TransactionListReportHeaderServiceModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string? TransactionType { get; set; } // null = ALL
        public string? TransactionTypeName { get; set; }
        public string? ItemCodePrefix { get; set; }
        public int TotalLineItems { get; set; }

        // Sum of every line's Qty*Price, each converted into the first line's own
        // currency - well-defined regardless of which type filter is applied (legacy
        // only ever showed this for GRN/GIN specifically; showing it for every
        // selection here is a deliberate improvement, not a fidelity gap).
        public decimal TotalValue { get; set; }
        public string? TotalValueCurrency { get; set; }
    }
}
