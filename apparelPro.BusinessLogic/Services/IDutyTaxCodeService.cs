using apparelPro.BusinessLogic.Services.Models.ImportExport.IDutyTaxCodeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IDutyTaxCodeService
    {
        Task<PaginationResult<DutyTaxCodeServiceModel>> GetDutyTaxCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<DutyTaxCodeServiceModel> GetDutyTaxCodeByCodeAsync(string code);
        Task<DutyTaxCodeServiceModel> AddDutyTaxCodeAsync(CreateDutyTaxCodeServiceModel createDutyTaxCodeServiceModel);
        Task UpdateDutyTaxCodeAsync(UpdateDutyTaxCodeServiceModel updateDutyTaxCodeServiceModel);
        Task DeleteDutyTaxCodeAsync(string code);
    }
}
