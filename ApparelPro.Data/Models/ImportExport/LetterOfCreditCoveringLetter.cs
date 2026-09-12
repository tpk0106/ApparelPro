namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_lclet.dbf (IE_LCLT1.PRG) - a fixed cover-letter
    // template with 7 yes/no paragraph checkboxes (some carrying an inline
    // fill-in value) plus 3 free-text paragraph pairs. Keyed the same way as
    // LetterOfCreditHeader (BankCode+LcNo), but legacy only offers BOC/SCB
    // for this letter type, not Peoples Bank.
    public class LetterOfCreditCoveringLetter
    {
        public int Id { get; set; }
        public string BankCode { get; set; } = "";
        public string LcNo { get; set; } = "";
        public DateOnly? LetterDate { get; set; }
        public string? ExportLcNo { get; set; }
        public string? Value { get; set; }
        public string? Item1 { get; set; }
        public string? Item2 { get; set; }
        public bool Box1Selected { get; set; }
        public bool Box2Selected { get; set; }
        public string? Attn1 { get; set; }
        public bool Box3Selected { get; set; }
        public string? Attn2 { get; set; }
        public bool Box4Selected { get; set; }
        public string? SampleLcNo { get; set; }
        public bool Box5Selected { get; set; }
        public bool Box6Selected { get; set; }
        public bool Box7Selected { get; set; }
        public decimal? Percentage { get; set; }
        public string? Box8Line1 { get; set; }
        public string? Box8Line2 { get; set; }
        public string? Box9Line1 { get; set; }
        public string? Box9Line2 { get; set; }
        public string? Box10Line1 { get; set; }
        public string? Box10Line2 { get; set; }
    }
}
