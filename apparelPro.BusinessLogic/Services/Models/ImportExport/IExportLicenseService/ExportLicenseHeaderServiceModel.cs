namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IExportLicenseService
{
    public class ExportLicenseHeaderServiceModel
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
