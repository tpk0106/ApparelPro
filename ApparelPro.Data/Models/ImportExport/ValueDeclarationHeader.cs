namespace ApparelPro.Data.Models.ImportExport
{
    // Matches the real Sri Lanka Customs 308A "Value Declaration" (VDF) form
    // the user supplied - not a migration of legacy ie_dec (IE_DEC1-5.PRG),
    // which is actually a Country-of-Origin/manufacturing-process
    // declaration, a different document entirely despite the similar
    // "Material Declaration Form" menu label.
    //
    // Two coexisting entry points share this one table (additive, by
    // explicit user instruction - "keep current entry as it is... we can
    // check and remove later separate entry"):
    //  - the original STANDALONE flow (Id-keyed, ExporterName/Address free
    //    text, ImporterCompanyAddressId, InvoiceNo/InvoiceDate as plain
    //    fields) - untouched, still served by the original endpoints.
    //  - the newer INVOICE-SCOPED flow (InvoiceNumber + CompanyAddressId for
    //    Exporter = our own company, Importer resolved from the linked
    //    Commercial Invoice's own Consignee via CommercialInvoiceService) -
    //    same pattern as Certificate of Origin/Boat Note, added alongside
    //    without removing the standalone fields/endpoints.
    public class ValueDeclarationHeader
    {
        public int Id { get; set; }

        // Invoice-scoped flow only.
        public string? InvoiceNumber { get; set; }
        public int? CompanyAddressId { get; set; }

        // Reference numbers printed at the top of Form 308A (customs-assigned).
        public string? Year { get; set; }
        public string? OfficeCode { get; set; }
        public string? SeriesLetter { get; set; }
        public string? CusDecNo { get; set; }

        // Standalone flow only.
        public string ExporterName { get; set; } = "";
        public string? ExporterAddress { get; set; }
        public string InvoiceNo { get; set; } = "";
        public DateOnly? InvoiceDate { get; set; }
        // Nullable: required for the standalone flow, unused/null for the
        // invoice-scoped flow (Importer there is resolved from the linked
        // invoice's own Consignee instead - see ValueDeclarationService).
        public int? ImporterCompanyAddressId { get; set; }

        public string? IndentingAgentName { get; set; }
        public string? IndentingAgentAddress { get; set; }
        public string? ImporterVatNo { get; set; }
        public string? DeclarantVatNo { get; set; }
        public string? SalesContractNo { get; set; }
        public DateOnly? SalesContractDate { get; set; }
        public decimal TotalInvoiceValue { get; set; }
        public string? NatureOfTransaction { get; set; }
        public string? CurrencyCode { get; set; }
        public string? TermsOfDeliveryCode { get; set; }

        // Box 12-15 relationship/circumstance questions.
        public bool? IsRelatedToSeller { get; set; }
        public bool? WasValueInfluencedByRelationship { get; set; }
        public bool? IsSaleSubjectToConditions { get; set; }
        public bool? HasPreviousImportsLast3Months { get; set; }
        public string? PreviousImportsDetails { get; set; }

        // Box 11 (a)-(j) costs not included in invoice value, per Article 8(1)/8(2).
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

        public string? SignatoryName { get; set; }
        public string? SignatoryTitle { get; set; }
        public DateOnly? SignatoryDate { get; set; }
        public string? SignatoryCompanyName { get; set; }
        public int? ContinuationSheetsCount { get; set; }

        // "For Office Use" - Appraiser/SC comments are one of "Satisfied" /
        // "Doubt" / "Suspect fraud" (matches the real form's fixed rating
        // scale under both boxes) - free text only for the reference
        // numbers, no real approval workflow (same scope decision as Boat
        // Note's Digital Verifications & Releases block).
        public string? AppraiserComments { get; set; }
        public string? ScComments { get; set; }
        public string? ValuationReferenceNo { get; set; }
        public string? CentralValuationEndorsement { get; set; }
    }
}
