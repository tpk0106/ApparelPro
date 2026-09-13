namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IExportLicenseService
{
    public class ExportLicenseDetailServiceModel
    {
        public ExportLicenseHeaderServiceModel Header { get; set; } = new();
        public List<ExportLicenseLineServiceModel> Lines { get; set; } = new();
    }
}
