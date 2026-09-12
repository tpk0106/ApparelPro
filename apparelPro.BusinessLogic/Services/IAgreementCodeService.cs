using apparelPro.BusinessLogic.Services.Models.ImportExport.IAgreementCodeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IAgreementCodeService
    {
        Task<PaginationResult<AgreementCodeServiceModel>> GetAgreementCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<AgreementCodeServiceModel> GetAgreementCodeByCodeAsync(string code);
        Task<AgreementCodeServiceModel> AddAgreementCodeAsync(CreateAgreementCodeServiceModel createAgreementCodeServiceModel);
        Task UpdateAgreementCodeAsync(UpdateAgreementCodeServiceModel updateAgreementCodeServiceModel);
        Task DeleteAgreementCodeAsync(string code);
    }
}
