namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICertificateOfOriginService
{
    public class SaveCertificateOfOriginServiceModel
    {
        public CertificateOfOriginHeaderServiceModel Header { get; set; } = null!;
        public List<CertificateOfOriginLineServiceModel> Lines { get; set; } = new();
    }
}
