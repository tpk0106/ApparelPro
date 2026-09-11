namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class CertificateOfOriginDetailAPIModel
    {
        public CertificateOfOriginHeaderAPIModel Header { get; set; } = null!;
        public List<CertificateOfOriginLineAPIModel> Lines { get; set; } = new();
    }
}
