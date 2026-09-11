namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class SaveCertificateOfOriginAPIModel
    {
        public CertificateOfOriginHeaderAPIModel Header { get; set; } = null!;
        public List<CertificateOfOriginLineAPIModel> Lines { get; set; } = new();
    }
}
