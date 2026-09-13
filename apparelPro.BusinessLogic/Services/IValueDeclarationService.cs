using apparelPro.BusinessLogic.Services.Models.ImportExport.IValueDeclarationService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IValueDeclarationService
    {
        Task<PaginationResult<ValueDeclarationHeaderServiceModel>> GetValueDeclarationsAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<ValueDeclarationDetailServiceModel?> GetByIdAsync(int id);
        Task<ValueDeclarationDetailServiceModel> SaveAsync(SaveValueDeclarationServiceModel serviceModel);
        Task<bool> DeleteAsync(int id);
        Task<ValueDeclarationPrintDetailsServiceModel?> GetPrintDetailsAsync(int id);

        // Invoice-scoped flow (one Value Declaration per Commercial
        // Invoice) - additive, alongside the standalone flow above.
        Task<ValueDeclarationDetailServiceModel?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<ValueDeclarationDetailServiceModel> SaveForInvoiceAsync(SaveValueDeclarationServiceModel serviceModel);
        Task<bool> DeleteByInvoiceNumberAsync(string invoiceNumber);
        Task<ValueDeclarationPrintDetailsServiceModel?> GetPrintDetailsByInvoiceNumberAsync(string invoiceNumber);
    }
}
