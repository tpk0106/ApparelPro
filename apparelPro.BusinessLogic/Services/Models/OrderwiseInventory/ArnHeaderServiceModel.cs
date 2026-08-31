namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ArnHeaderServiceModel
    {
        public DateTime TransactionDate { get; set; }

        // Legacy IN_ARN4.PRG "Sub Cont." field (m_subcnt) - validated against
        // SubContractors before commit.
        public string SubContractorCode { get; set; } = null!;

        public string? InvoiceNumber { get; set; }

        // Legacy IN_ARN4.PRG header-level "Currency" field (m_curr) - the currency the
        // Price on every line is entered in, converted per-line into that item's own
        // OrderwiseStockMaster currency on commit (mirrors legacy's curconv() call).
        public string Currency { get; set; } = null!;

        // Server-assigned during commit via DocumentSequences (NoteType "ARN") - never
        // trusted from client input, same convention as every other note type.
        public string ArnNumber { get; set; } = null!;
    }
}
