using apparelPro.BusinessLogic.Services.Models.Reference.IPortDestinationService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPortDestinationService
    {
        Task<PaginationResult<PortDestinationServiceModel>> GetPortDestinationsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);      
        Task<PortDestinationServiceModel> GetPortDestinationByIdAndCountryCodeAsync(int id, string countryCode);
       // Task<IEnumerable<PortDestinationServiceModel>> FilterBuyerByCodeAsync(string filter, int pageNumber, int pageSize);
        Task<PortDestinationServiceModel> AddPortDestinationAsync(CreatePortDestinationServiceModel createBuyerServiceModel);
        Task UpdatePortDestinationAsync(UpdatePortDestinationServiceModel updateCountryServiceModel);
        Task DeletePortDestinationAsync(int id, string countryCode);
    }
}
