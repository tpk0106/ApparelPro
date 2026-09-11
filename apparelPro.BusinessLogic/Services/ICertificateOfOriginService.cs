using apparelPro.BusinessLogic.Services.Models.ImportExport.ICertificateOfOriginService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICertificateOfOriginService
    {
        Task<CertificateOfOriginDetailServiceModel?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<CertificateOfOriginDetailServiceModel> SaveAsync(SaveCertificateOfOriginServiceModel serviceModel);
        Task<bool> DeleteAsync(string invoiceNumber);
        Task<CertificateOfOriginPrintDetailsServiceModel?> GetPrintDetailsAsync(string invoiceNumber);
    }
}
