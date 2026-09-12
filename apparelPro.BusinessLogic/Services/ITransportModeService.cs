using apparelPro.BusinessLogic.Services.Models.ImportExport.ITransportModeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ITransportModeService
    {
        Task<PaginationResult<TransportModeServiceModel>> GetTransportModesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<TransportModeServiceModel> GetTransportModeByCodeAsync(string code);
        Task<TransportModeServiceModel> AddTransportModeAsync(CreateTransportModeServiceModel createTransportModeServiceModel);
        Task UpdateTransportModeAsync(UpdateTransportModeServiceModel updateTransportModeServiceModel);
        Task DeleteTransportModeAsync(string code);
    }
}
