using apparelPro.BusinessLogic.Services.Models.Reference.ISeasonService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    // Season master data is seeded from legacy od_sea.dbf (see SeasonConfig) and used by
    // both the Order Confirmation Routine's Season dropdown and the Year/Season Wise
    // Orders report.
    public interface ISeasonService
    {
        Task<PaginationResult<SeasonServiceModel>> GetSeasonsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<SeasonServiceModel?> GetSeasonByCodeAsync(string code);
        Task<SeasonServiceModel> AddSeasonAsync(CreateSeasonServiceModel createSeasonServiceModel);
        Task UpdateSeasonAsync(UpdateSeasonServiceModel updateSeasonServiceModel);
        Task DeleteSeasonAsync(string code);
    }
}
