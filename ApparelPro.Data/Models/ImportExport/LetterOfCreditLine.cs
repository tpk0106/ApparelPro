namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_lc2.dbf. v1 is simple free-entry lines (no
    // allocation lock against Purchase Order detail lines like legacy's
    // od_podet.bank_lcno stamping - deferred, see
    // project_ie_legacy_data_migration_todo memory for related follow-ups).
    public class LetterOfCreditLine
    {
        public int Id { get; set; }
        public string BankCode { get; set; } = "";
        public string LcNo { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Currency { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public string? BtnNo { get; set; }
    }
}
