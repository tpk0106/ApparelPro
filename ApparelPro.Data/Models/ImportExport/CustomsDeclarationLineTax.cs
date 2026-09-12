namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_cusd3.dbf (tax sub-lines per item). Keyed by CusNo+Item+Tax.
    public class CustomsDeclarationLineTax
    {
        public int Id { get; set; }
        public string CusNo { get; set; } = "";
        public string Item { get; set; } = "";
        public string? TaxCode { get; set; }
        public string? BaseCode { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Exempted { get; set; }
        public decimal? Payable { get; set; }
    }
}
