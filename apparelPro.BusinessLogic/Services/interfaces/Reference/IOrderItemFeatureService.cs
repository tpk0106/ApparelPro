using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemFeatureService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services.interfaces.Reference
{
    public interface IOrderItemFeatureService
    {
        Task<PaginationResult<OrderItemFeatureMappingServiceModel>> GetOrderItemFeaturesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder,
            string? filterColumn, string? filterQuery);
        Task<OrderItemFeatureMappingServiceModel?> GetOrderItemFeatureAsync(string stockCode, string itemCode);
        Task<bool> DoesOrderItemFeatureExistAsync(string stockCode, string itemCode);
        Task<OrderItemFeatureMappingServiceModel> AddOrderItemFeatureAsync(CreateOrderItemFeatureMappingServiceModel createOrderItemFeatureMappingServiceModel);
        Task UpdateOrderItemFeatureAsync(UpdateOrderItemFeatureMappingServiceModel updateOrderItemFeatureMappingServiceModel);
        Task DeleteOrderItemFeatureAsync(string stockCode, string itemCode);
    }
}
