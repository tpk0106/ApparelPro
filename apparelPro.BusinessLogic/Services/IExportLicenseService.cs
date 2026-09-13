using apparelPro.BusinessLogic.Services.Models.ImportExport.IExportLicenseService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IExportLicenseService
    {
        Task<PaginationResult<ExportLicenseHeaderServiceModel>> GetExportLicensesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<ExportLicenseDetailServiceModel?> GetByIdAsync(int id);
        Task<ExportLicenseDetailServiceModel> SaveAsync(SaveExportLicenseServiceModel serviceModel);
        Task<bool> DeleteAsync(int id);
        Task<ExportLicensePrintDetailsServiceModel?> GetPrintDetailsAsync(int id);
    }
}
