namespace ApparelPro.Data.Models.OrderManagement.Shipments
{
    // Maps legacy ie_coinv - the export commercial invoice header (one row per
    // invoice, referenced by CommercialInvoiceLine.InvoiceNumber).
    public class CommercialInvoiceHeader
    {
        public string InvoiceNumber { get; set; } = null!; // invo_no
        public DateTime InvoiceDate { get; set; } // invo_dt

        public int BuyerCode { get; set; } // buyer
        public string? DocumentaryBuyerCode { get; set; } // doc_buyer
        public string? NotifyPartyCode { get; set; } // notify
        public string? ConsigneeCode { get; set; } // consignee

        public string? LoadPortCode { get; set; } // load
        public string? DestinationCode { get; set; } // dest
        public string? ContinuingDestinationCode { get; set; } // cont_dest
        public string? CarrierCode { get; set; } // carrier
        public DateTime? ShipDate { get; set; } // ship_dt

        public string? LcNumber { get; set; } // lc_no
        public DateTime? LcDate { get; set; } // lc_dt
        public string? AssessmentNumber { get; set; } // ass_no
        public string? IssuingBankCode { get; set; } // iss_bank

        public string? Remark1 { get; set; } // rem1
        public string? Remark2 { get; set; } // rem2
        public string? Remark3 { get; set; } // rem3
        public string? Detail { get; set; } // detail (legacy memo field)

        public string? CurrencyCode { get; set; } // curr
        public string? TradeTermCode { get; set; } // t_term
        public string? TradeTermLine1 { get; set; } // t_term1
        public string? TradeTermLine2 { get; set; } // t_term2
        public string? TradeTermLine3 { get; set; } // t_term3
    }
}
