namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_lc.dbf exactly - a single flat table shared across
    // all three bank formats (BOC/SCB/Peoples Bank). The legacy screen
    // (IE_LCBC1.PRG) just shows/labels a different subset of these same
    // fields per bank ("tom"/"dick"/"duck" functions); it never was three
    // separate schemas.
    public class LetterOfCreditHeader
    {
        public int Id { get; set; }
        public string BankCode { get; set; } = "";
        public string LcNo { get; set; } = "";
        public string? CreditNo { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public string? ExpiryPlaceCode { get; set; }
        public DateOnly? OpeningDate { get; set; }
        public int? BeneficiaryCode { get; set; }
        public string? IssuedBy { get; set; }
        public string? NotifyPartyCode { get; set; }
        public string? LicenceType { get; set; }
        public string? LicenceNo { get; set; }
        public string? TransferableCredit { get; set; }
        public string? ConfirmedCredit { get; set; }
        public string? PartShipment { get; set; }
        public string? Transhipment { get; set; }
        public string? InsuranceCoverage { get; set; }
        public string? ShipmentTerm { get; set; }
        public string? ShipmentTermOther { get; set; }
        public string? BillOfLadingIssued { get; set; }
        public string? FreightPayment { get; set; }
        public string? AirwayDocumentType { get; set; }
        public string? AdditionalConditions { get; set; }
        public string? ExtraConditions { get; set; }
        public string? CreditBy { get; set; }
        public string? BeneficiaryDraft { get; set; }
        public string? InsuranceClause { get; set; }
        public string? CountryOfOriginCode { get; set; }
        public string? ShipmentFromCode { get; set; }
        public string? TransportTo { get; set; }
        public decimal? InsurancePercent { get; set; }
        public string? InsuranceValueCurrency { get; set; }
        public string? CertifiedMailCopies { get; set; }
        public string? DocumentPresentationDays { get; set; }
        public string? ShipmentTermCustomLabel { get; set; }
        public string? AccountNo { get; set; }
        public string? Branch { get; set; }
        public string? CreditAvailableWith { get; set; }
        public string? CreditDocuments { get; set; }
        public string? ConformityWith { get; set; }
        public string? InsuranceRemarks { get; set; }
        public string? TenorDays { get; set; }
        public string? DrawnOn { get; set; }
        public string? CiCopies { get; set; }
        public DateOnly? TenorDate { get; set; }
        public DateOnly? NotLaterThanDate { get; set; }
        public string? BeneficiaryCountryCode { get; set; }
        public string? AdvisingBankCode { get; set; }
        public string? InvoiceSelection { get; set; }
        public string? PackingSpecification { get; set; }
        public string? MarineBillOfLading { get; set; }
        public string? MarineBillOfLadingConsignee { get; set; }
        public string? AirWaybill { get; set; }
        public string? AirWaybillConsignee { get; set; }
        public string? OtherDocuments { get; set; }
        public string? BankSentTo { get; set; }
        public string? ImportPermitNo { get; set; }
        public string? ImportContractNo { get; set; }
        public string? IncomeTaxNo { get; set; }
        public string? BttReferenceNo { get; set; }
        public DateOnly? ImportValidityDate { get; set; }
    }
}
