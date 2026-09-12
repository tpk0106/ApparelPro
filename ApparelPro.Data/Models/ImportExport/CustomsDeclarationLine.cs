namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_cusd2.dbf (CUSDEC item lines). Keyed by CusNo+Item.
    public class CustomsDeclarationLine
    {
        public int Id { get; set; }
        public string CusNo { get; set; } = "";
        public string Item { get; set; } = "";
        public string? CustomsProcedureCode { get; set; }
        public string? CommodityCode { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public string? SupplementaryUnitCode { get; set; }
        public decimal? SupplementaryQty { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? Fob { get; set; }
        public decimal? Freight { get; set; }
        public decimal? Insurance { get; set; }
        public decimal? Other { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? CountryCode { get; set; }
        public string? LicenceNo { get; set; }
        public string? AgreementCode { get; set; }
        public decimal? QtyDeducted { get; set; }
        public string? Value { get; set; }
        public string? AnyOther { get; set; }
        public string? Detail { get; set; }
    }
}
