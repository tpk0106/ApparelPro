using apparelPro.BusinessLogic.Services.Models.Production.ISectionService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ISectionService
    {
        Task<PaginationResult<SectionServiceModel>> GetSectionsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<List<SectionServiceModel>> GetAllSectionsAsync();
        Task<SectionServiceModel> AddSectionAsync(CreateSectionServiceModel createServiceModel);
        Task UpdateSectionAsync(UpdateSectionServiceModel updateServiceModel);
        Task DeleteSectionAsync(string code);
    }
}
