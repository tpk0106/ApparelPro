using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IStyleDetailsService
    {
        Task<PaginationResult<StyleDetailsServiceModel>> GetStyleDetailsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);        
        Task<IEnumerable<StyleDetailsServiceModel>> GetStyleDetailsByPageNumberAsync(int pageNumber, int pageSize);
        Task<IEnumerable<StyleDetailsServiceModel>> FilterStyleDetailsByCodeAsync(string filter, int pageNumber, int pageSize);        
        Task<StyleDetailsServiceModel> GetStyleDetailsByBuyerOrderTypeStyleAsync(int buyer, string order, int type, string style);
        Task<IEnumerable<StyleDetailsServiceModel>> GetStyleDetailsByBuyerOrderTypeAsync(int buyerCode, string order, int typeCode);
        Task<PaginationResult<StyleDetailsServiceModel>> GetStyleDetailsByBuyerOrderAsync(int buyer, string order, int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);        
        Task<bool> DoesStyleDetailsExistAsync(string code);
        Task<StyleDetailsServiceModel> AddStyleDetailsAsync(CreateStyleDetailsServiceModel createCountryServiceModel, string? currentUserEmail = null);
        Task UpdateStyleDetailsAsync(UpdateStyleDetailsServiceModel updateCountryServiceModel, string? currentUserEmail = null);
        Task DeleteStyleDetailsAsync(int buyer, string order, int type, string style);
        Task<StyleApprovalDetailsServiceModel> GetEstimateApprovalUserNameAsync(int buyer, string order, int type, string style);
        Task<StyleTotalsServiceModel> GetStyleTotalsAsync(int buyerCode, string order);
    }
}
