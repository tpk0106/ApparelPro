using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPurchaseOrderService
    {
        Task<PaginationResult<PurchaseOrderServiceModel>> GetPurchaseOrderAsync(int pageNumber, int pageSize, 
            string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<PurchaseOrderServiceModel> GetPurchaseOrderByBuyerAndOrderAsync(int buyer, string ordr);
        Task<PurchaseOrderServiceModel> AddPurchaseOrderAsync(CreatePurchaseOrderServiceModel createCurrencyServiceModel);
        Task UpdatePurchaseOrderAsync(UpdatePurchaseOrderServiceModel updateCurrencyServiceModel);
        Task DeletePurchaseOrderAsync(string code);
    }
}
