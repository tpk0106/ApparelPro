using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemCatalogService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services.interfaces.Reference
{
    // Order Items Catalog (od_itm) - the Stock/Item master list entered via
    // [F1] help in legacy (e.g. OD_AITM1.PRG: Stock via F1, Item Code typed and
    // validated against this same catalog). Backed by the existing OrderItems
    // table/OrderItem entity that GetMaterialCatalogAsync already reads from -
    // this is the missing "add a new catalog entry" screen for that same data.
    public interface IOrderItemCatalogService
    {
        Task<PaginationResult<OrderItemCatalogServiceModel>> GetOrderItemCatalogAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<OrderItemCatalogServiceModel?> GetOrderItemCatalogByCodeAsync(string stockCode, string itemCode);
        Task<bool> DoesOrderItemCatalogExistAsync(string stockCode, string itemCode);
        Task<OrderItemCatalogServiceModel> AddOrderItemCatalogAsync(CreateOrderItemCatalogServiceModel createOrderItemCatalogServiceModel);
        Task UpdateOrderItemCatalogAsync(UpdateOrderItemCatalogServiceModel updateOrderItemCatalogServiceModel);
        Task DeleteOrderItemCatalogAsync(string stockCode, string itemCode);
    }
}
