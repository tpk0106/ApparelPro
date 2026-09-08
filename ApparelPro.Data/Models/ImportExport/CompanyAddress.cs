namespace ApparelPro.Data.Models.ImportExport
{
    // Maps legacy ie_setup - NOT a singleton (confirmed against production
    // data: 3 real, distinct address records). Import/Export documents like
    // Certificate of Origin reference one of these by AddressNo.
    public class CompanyAddress
    {
        public int Id { get; set; }
        public int AddressNo { get; set; }
        public string CompanyName { get; set; } = "";
        public string Address1 { get; set; } = "";
        public string Address2 { get; set; } = "";
        public string City { get; set; } = "";
        public string PostCode { get; set; } = "";
        public string Country { get; set; } = "";
        public string TelNos { get; set; } = "";
        public string FaxNos { get; set; } = "";
        public string TinNo { get; set; } = "";
        public string ExportRegNo { get; set; } = "";
    }
}
