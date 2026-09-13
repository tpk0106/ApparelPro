namespace ApparelPro.Data.Models.ImportExport
{
    // Matches the real Sri Lanka "Application for an Export Control License"
    // form (Department of Imports & Exports Control) the user supplied - NOT
    // a migration of legacy ie_elic2.prg, which is actually a quota-based
    // print artifact (pulls Quota Year/Country/Category + Certificate of
    // Origin data) for the old MFA quota system - a different document
    // entirely, and quota is industry-dead (see feedback_quota_module_hold).
    // Standalone document (own list, not tied to CommercialInvoiceHeader) -
    // the real form has no Invoice No. field at all; it's a general license
    // application, not scoped to a specific shipment. Applicant (boxes 1-7)
    // is always our own company (CompanyAddress).
    public class ExportLicenseHeader
    {
        public int Id { get; set; }

        public int CompanyAddressId { get; set; }
        public string? ApplicantType { get; set; }
        public string? BusinessRegistrationNo { get; set; }
        public string? VatRegistrationNo { get; set; }
        public string? Telephone { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? ApplicantIdOfficeUse { get; set; }

        public string? LicenseType { get; set; }
        public string? ExchangeType { get; set; }
        public string? CommercialType { get; set; }
        public string? BankCode { get; set; }
        public string? ModeOfPayment { get; set; }
        public string? ModeOfTransportation { get; set; }

        public int? Consignee1BuyerCode { get; set; }
        public int? Consignee2BuyerCode { get; set; }

        public string? PurposeOfExportation { get; set; }
        public string? UseOfCommodity { get; set; }

        public DateOnly? SignatoryDate { get; set; }
    }
}
