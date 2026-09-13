namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IExportLicenseService
{
    public class ExportLicensePrintDetailsServiceModel
    {
        public ExportLicenseHeaderServiceModel Header { get; set; } = null!;
        public List<ExportLicenseLineServiceModel> Lines { get; set; } = new();
        public string ApplicantCompanyName { get; set; } = "";
        public string ApplicantAddress1 { get; set; } = "";
        public string ApplicantAddress2 { get; set; } = "";
        public string ApplicantCityPostCodeCountry { get; set; } = "";
        public string BankName { get; set; } = "";
        public string Consignee1Name { get; set; } = "";
        public string Consignee1Address { get; set; } = "";
        public string Consignee2Name { get; set; } = "";
        public string Consignee2Address { get; set; } = "";
    }
}
