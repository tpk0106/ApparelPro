namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class CommercialInvoiceLineAPIModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string NewOrder { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Balance { get; set; }
        public string QuotaCategory { get; set; } = "";
        public string FromYearMonth { get; set; } = "";
        public string ToYearMonth { get; set; } = "";
        public string QuotaCountry { get; set; } = "";
        public string PackingMedia { get; set; } = "";
    }
}
