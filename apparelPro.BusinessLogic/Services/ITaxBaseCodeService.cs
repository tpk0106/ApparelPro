using apparelPro.BusinessLogic.Services.Models.ImportExport.ITaxBaseCodeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ITaxBaseCodeService
    {
        Task<PaginationResult<TaxBaseCodeServiceModel>> GetTaxBaseCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<TaxBaseCodeServiceModel> GetTaxBaseCodeByCodeAsync(string code);
        Task<TaxBaseCodeServiceModel> AddTaxBaseCodeAsync(CreateTaxBaseCodeServiceModel createTaxBaseCodeServiceModel);
        Task UpdateTaxBaseCodeAsync(UpdateTaxBaseCodeServiceModel updateTaxBaseCodeServiceModel);
        Task DeleteTaxBaseCodeAsync(string code);
    }
}
