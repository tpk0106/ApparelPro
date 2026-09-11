namespace ApparelPro.Data.Models.ImportExport
{
    // Matches the real Ceylon Chamber of Commerce "Certificate of Origin"
    // form (EXP 5) currently in use - not legacy ie_co (a different,
    // quota-linked DOS-era certificate that this migration deliberately does
    // not reproduce; quota is dead elsewhere in this project). One
    // certificate per Commercial Invoice.
    public class CertificateOfOriginHeader
    {
        public int Id { get; set; }
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
