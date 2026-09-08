using apparelPro.BusinessLogic.Services.Models.ImportExport.ICommercialInvoiceService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICommercialInvoiceService
    {
        Task<PaginationResult<CommercialInvoiceHeaderServiceModel>> GetInvoicesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<CommercialInvoiceDetailServiceModel?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<CommercialInvoiceDetailServiceModel> SaveAsync(SaveCommercialInvoiceServiceModel serviceModel);
        Task<bool> DeleteAsync(string invoiceNumber);
        Task<CommercialInvoicePrintDetailsServiceModel?> GetPrintDetailsAsync(string invoiceNumber);
    }
}
