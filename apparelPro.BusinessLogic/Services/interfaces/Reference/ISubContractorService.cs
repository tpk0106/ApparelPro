using apparelPro.BusinessLogic.Services.Models.Reference.ISubContractorService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services.interfaces.Reference
{
    public interface ISubContractorService
    {
        Task<PaginationResult<SubContractorServiceModel>> GetSubContractorsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<SubContractorServiceModel?> GetSubContractorByCodeAsync(string code);
        Task<bool> DoesSubContractorExistAsync(string code);
        Task<SubContractorServiceModel> AddSubContractorAsync(CreateSubContractorServiceModel createSubContractorServiceModel);
        Task UpdateSubContractorAsync(UpdateSubContractorServiceModel updateSubContractorServiceModel);
        Task DeleteSubContractorAsync(string code);
    }
}
