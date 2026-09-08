namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICompanyAddressService
{
    public class CompanyAddressServiceModel
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
