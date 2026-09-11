namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICertificateOfOriginService
{
    // Everything the printed Certificate of Origin (EXP 5) needs beyond the
    // header's own fields - the Consignor/Exporter block from CompanyAddress
    // (box 1) and the Consignee resolved from the linked Commercial
    // Invoice's own Buyer/Consignee (box 3), same resolution
    // CommercialInvoiceService.GetPrintDetailsAsync already does.
    public class CertificateOfOriginPrintDetailsServiceModel
    {
        public CertificateOfOriginHeaderServiceModel Header { get; set; } = null!;
        public List<CertificateOfOriginLineServiceModel> Lines { get; set; } = new();
        public string ExporterCompanyName { get; set; } = "";
        public string ExporterAddress1 { get; set; } = "";
        public string ExporterAddress2 { get; set; } = "";
        public string ExporterCityPostCodeCountry { get; set; } = "";
        public string ConsigneeName { get; set; } = "";
        public List<string> ConsigneeAddressLines { get; set; } = new();
    }
}
