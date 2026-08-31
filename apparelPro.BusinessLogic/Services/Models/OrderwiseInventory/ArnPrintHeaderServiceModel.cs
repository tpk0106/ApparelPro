namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ArnPrintHeaderServiceModel
    {
        public string ArnNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // Basis - legacy prints only the first line's store_cd
        public string? InvoiceNumber { get; set; }
        public string SubContractorCode { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public decimal TotalValue { get; set; }

        // Legacy IN_ARN2.PRG prints the current system date/time on every print run,
        // not the original transaction date - the note can be reprinted on demand.
        public DateTime PrintedOn { get; set; }
    }
}
