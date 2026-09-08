namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICommercialInvoiceService
{
    public class CommercialInvoiceHeaderServiceModel
    {
        public string InvoiceNumber { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }
        public int BuyerCode { get; set; }
        public string? DocumentaryBuyerCode { get; set; }
        public string? NotifyPartyCode { get; set; }
        public string? ConsigneeCode { get; set; }
        public string? LoadPortCode { get; set; }
        public string? DestinationCode { get; set; }
        public string? ContinuingDestinationCode { get; set; }
        public string? CarrierCode { get; set; }
        public DateTime? ShipDate { get; set; }
        public string? LcNumber { get; set; }
        public DateTime? LcDate { get; set; }
        public string? AssessmentNumber { get; set; }
        public string? IssuingBankCode { get; set; }
        public string? Remark1 { get; set; }
        public string? Remark2 { get; set; }
        public string? Remark3 { get; set; }
        public string? Detail { get; set; }
        public string? CurrencyCode { get; set; }
        public string? TradeTermCode { get; set; }
        public string? TradeTermLine1 { get; set; }
        public string? TradeTermLine2 { get; set; }
        public string? TradeTermLine3 { get; set; }
    }
}
