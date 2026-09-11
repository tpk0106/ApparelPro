namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICertificateOfOriginService
{
    public class CertificateOfOriginDetailServiceModel
    {
        public CertificateOfOriginHeaderServiceModel Header { get; set; } = null!;
        public List<CertificateOfOriginLineServiceModel> Lines { get; set; } = new();
    }
}
