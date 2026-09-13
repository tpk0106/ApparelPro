namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IValueDeclarationService
{
    public class ValueDeclarationHeaderServiceModel
    {
        public int Id { get; set; }
        public string? InvoiceNumber { get; set; }
        public int? CompanyAddressId { get; set; }
        public string? Year { get; set; }
        public string? OfficeCode { get; set; }
        public string? SeriesLetter { get; set; }
        public string? CusDecNo { get; set; }
        public string ExporterName { get; set; } = "";
        public string? ExporterAddress { get; set; }
        public string? IndentingAgentName { get; set; }
        public string? IndentingAgentAddress { get; set; }
        public string? ImporterVatNo { get; set; }
        public string? DeclarantVatNo { get; set; }
        public string? SalesContractNo { get; set; }
        public DateOnly? SalesContractDate { get; set; }
        public string InvoiceNo { get; set; } = "";
        public DateOnly? InvoiceDate { get; set; }
        public decimal TotalInvoiceValue { get; set; }
        public string? NatureOfTransaction { get; set; }
        public string? CurrencyCode { get; set; }
        public string? TermsOfDeliveryCode { get; set; }
        public bool? IsRelatedToSeller { get; set; }
        public bool? WasValueInfluencedByRelationship { get; set; }
        public bool? IsSaleSubjectToConditions { get; set; }
        public bool? HasPreviousImportsLast3Months { get; set; }
        public string? PreviousImportsDetails { get; set; }
        public decimal BrokerageCommission { get; set; }
        public decimal CostOfContainers { get; set; }
        public decimal PackingCosts { get; set; }
        public decimal CostOfGoodsSuppliedByBuyer { get; set; }
        public decimal RoyaltiesLicenseFees { get; set; }
        public decimal ProceedsToSeller { get; set; }
        public decimal LoadingHandlingCharges { get; set; }
        public decimal Insurance { get; set; }
        public decimal Freight { get; set; }
        public decimal OtherPayments { get; set; }
        public string? TermsOfPaymentCode { get; set; }
        public string? PortOfShipmentCode { get; set; }
        public string? AwbBlNo { get; set; }
        public DateOnly? AwbBlDate { get; set; }
        public int? ImporterCompanyAddressId { get; set; }
        public string? SignatoryName { get; set; }
        public string? SignatoryTitle { get; set; }
        public DateOnly? SignatoryDate { get; set; }
        public string? SignatoryCompanyName { get; set; }
        public int? ContinuationSheetsCount { get; set; }
        public string? AppraiserComments { get; set; }
        public string? ScComments { get; set; }
        public string? ValuationReferenceNo { get; set; }
        public string? CentralValuationEndorsement { get; set; }
    }
}
