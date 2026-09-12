namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_cusd1.dbf (CUSDEC I/II header). CusNo is the
    // legacy primary key (user-entered, not auto-generated).
    public class CustomsDeclarationHeader
    {
        public string CusNo { get; set; } = "";
        public string? ExporterCode { get; set; }
        public string? BoiRegNo { get; set; }
        public string? ConsigneeCode { get; set; }
        public string? NotifyPartyCode { get; set; }
        public string? DeclarantCode { get; set; }
        public string? ClearanceOfficeCode { get; set; }
        public string? FrontierOfficeCode { get; set; }
        public string? CountryOfConsignmentCode { get; set; }
        public string? LocationOfGoods { get; set; }
        public string? CountryOfOriginCode { get; set; }
        public string? CountryOfDestinationCode { get; set; }
        public string? WarehouseNo { get; set; }
        public string? WarehousePeriod { get; set; }
        public string? PrecedingDocNo { get; set; }
        public string? VoyageNo { get; set; }
        public DateOnly? VoyageDate { get; set; }
        public string? BlAwbNo { get; set; }
        public string? PaymentTermCode { get; set; }
        public string? DeliveryTermCode { get; set; }
        public string? Vessel { get; set; }
        public string? PortOfLoadingCode { get; set; }
        public string? TransportModeCode { get; set; }
        public string? PrepaymentAccountName { get; set; }
        public string? PrepaymentAccountNo { get; set; }
        public string? PortOfDischargeCode { get; set; }
        public string? PlaceOfDeliveryCode { get; set; }
        public string? BankCode { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Remark1 { get; set; }
        public string? Remark2 { get; set; }
        public string? Remark3 { get; set; }
        public string? Remark4 { get; set; }
        public string? DeclarantName { get; set; }
        public string? SubmittedByName { get; set; }
    }
}
