namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class SaveExportLicenseAPIModel
    {
        public ExportLicenseHeaderAPIModel Header { get; set; } = new();
        public List<ExportLicenseLineAPIModel> Lines { get; set; } = new();
    }
}
