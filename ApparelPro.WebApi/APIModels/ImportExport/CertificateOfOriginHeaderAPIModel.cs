namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class CertificateOfOriginHeaderAPIModel
    {
        public string InvoiceNumber { get; set; } = "";
        public string RefNo { get; set; } = "";
        public int CompanyAddressId { get; set; }
        public string CountryOfOrigin { get; set; } = "LK";
        public string? PortOfLoading { get; set; }
        public string? OtherRemarks { get; set; }
        public string? CompetentAuthorityName { get; set; }
        public string? IssuePlace { get; set; }
        public DateOnly? IssueDate { get; set; }
        public string? RequestSubmittedBy { get; set; }
    }
}
