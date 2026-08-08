using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemFeatureService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class OrderItemFeatureService : IOrderItemFeatureService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public OrderItemFeatureService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
        }

        public async Task<OrderItemFeatureMappingServiceModel> AddOrderItemFeatureAsync(CreateOrderItemFeatureMappingServiceModel createOrderItemFeatureMappingServiceModel)
        {
            var exists = await _apparelProDbContext.OrderItemFeatures
                .AnyAsync(f => f.StockCode == createOrderItemFeatureMappingServiceModel.StockCode
                             && f.ItemCode == createOrderItemFeatureMappingServiceModel.ItemCode);
            if (exists)
            {
                throw new InvalidOperationException("An Order Item Feature mapping already exists for this Stock Code and Item Code.");
            }

            var stockItemExists = await _apparelProDbContext.StockItems
                .AnyAsync(si => si.StockCode == createOrderItemFeatureMappingServiceModel.StockCode
                              && si.ItemCode == createOrderItemFeatureMappingServiceModel.ItemCode);
            if (!stockItemExists)
            {
                throw new InvalidOperationException("The given Stock Code and Item Code does not exist in the Stock Item master.");
            }

            await ValidateFeatureCodesExistAsync(
                createOrderItemFeatureMappingServiceModel.Feature1Type,
                createOrderItemFeatureMappingServiceModel.Feature2Type,
                createOrderItemFeatureMappingServiceModel.Feature3Type,
                createOrderItemFeatureMappingServiceModel.Feature4Type);

            var orderItemFeatureDbModel = new OrderItemFeature
            {
                StockCode = createOrderItemFeatureMappingServiceModel.StockCode,
                ItemCode = createOrderItemFeatureMappingServiceModel.ItemCode,
                Feature1Type = createOrderItemFeatureMappingServiceModel.Feature1Type,
                Feature2Type = createOrderItemFeatureMappingServiceModel.Feature2Type,
                Feature3Type = createOrderItemFeatureMappingServiceModel.Feature3Type,
                Feature4Type = createOrderItemFeatureMappingServiceModel.Feature4Type,
                CostPerUnit = createOrderItemFeatureMappingServiceModel.CostPerUnit,
            };

            _apparelProDbContext.OrderItemFeatures.Add(orderItemFeatureDbModel);
            await _apparelProDbContext.SaveChangesAsync();

            return await MapToServiceModelWithNamesAsync(orderItemFeatureDbModel);
        }

        public async Task DeleteOrderItemFeatureAsync(string stockCode, string itemCode)
        {
            var orderItemFeatureDbModel = await _apparelProDbContext.OrderItemFeatures
                .Where(f => f.StockCode == stockCode && f.ItemCode == itemCode)
                .FirstOrDefaultAsync();

            if (orderItemFeatureDbModel == null)
            {
                throw new InvalidOperationException("Order Item Feature mapping is not available for the given Stock Code and Item Code.");
            }

            _apparelProDbContext.OrderItemFeatures.Remove(orderItemFeatureDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<bool> DoesOrderItemFeatureExistAsync(string stockCode, string itemCode)
        {
            return await _apparelProDbContext.OrderItemFeatures
                .AnyAsync(f => f.StockCode == stockCode && f.ItemCode == itemCode);
        }

        public async Task<OrderItemFeatureMappingServiceModel?> GetOrderItemFeatureAsync(string stockCode, string itemCode)
        {
            var orderItemFeatureDbModel = await _apparelProDbContext.OrderItemFeatures
                .AsNoTracking()
                .Where(f => f.StockCode == stockCode && f.ItemCode == itemCode)
                .FirstOrDefaultAsync();

            if (orderItemFeatureDbModel == null)
            {
                return null;
            }

            return await MapToServiceModelWithNamesAsync(orderItemFeatureDbModel);
        }

        public async Task<PaginationResult<OrderItemFeatureMappingServiceModel>> GetOrderItemFeaturesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<OrderItemFeature> orderItemFeaturesPagination = _apparelProDbContext.OrderItemFeatures.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(OrderItemFeature));
                orderItemFeaturesPagination = orderItemFeaturesPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = await orderItemFeaturesPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                orderItemFeaturesPagination = orderItemFeaturesPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            else
            {
                orderItemFeaturesPagination = orderItemFeaturesPagination.OrderBy(f => f.StockCode).ThenBy(f => f.ItemCode);
            }

            orderItemFeaturesPagination = orderItemFeaturesPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbOrderItemFeatures = await orderItemFeaturesPagination.ToListAsync();

            var orderItemFeatureServiceModels = new List<OrderItemFeatureMappingServiceModel>();
            foreach (var orderItemFeatureDbModel in filteredDbOrderItemFeatures)
            {
                orderItemFeatureServiceModels.Add(await MapToServiceModelWithNamesAsync(orderItemFeatureDbModel));
            }

            return new PaginationResult<OrderItemFeatureMappingServiceModel>(pageSize, pageNumber, counter, orderItemFeatureServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateOrderItemFeatureAsync(UpdateOrderItemFeatureMappingServiceModel updateOrderItemFeatureMappingServiceModel)
        {
            var orderItemFeatureDbModel = await _apparelProDbContext.OrderItemFeatures
                .Where(f => f.StockCode == updateOrderItemFeatureMappingServiceModel.StockCode
                          && f.ItemCode == updateOrderItemFeatureMappingServiceModel.ItemCode)
                .FirstOrDefaultAsync();

            if (orderItemFeatureDbModel == null)
            {
                throw new InvalidOperationException("Order Item Feature mapping is not available for the given Stock Code and Item Code.");
            }

            await ValidateFeatureCodesExistAsync(
                updateOrderItemFeatureMappingServiceModel.Feature1Type,
                updateOrderItemFeatureMappingServiceModel.Feature2Type,
                updateOrderItemFeatureMappingServiceModel.Feature3Type,
                updateOrderItemFeatureMappingServiceModel.Feature4Type);

            orderItemFeatureDbModel.Feature1Type = updateOrderItemFeatureMappingServiceModel.Feature1Type;
            orderItemFeatureDbModel.Feature2Type = updateOrderItemFeatureMappingServiceModel.Feature2Type;
            orderItemFeatureDbModel.Feature3Type = updateOrderItemFeatureMappingServiceModel.Feature3Type;
            orderItemFeatureDbModel.Feature4Type = updateOrderItemFeatureMappingServiceModel.Feature4Type;
            orderItemFeatureDbModel.CostPerUnit = updateOrderItemFeatureMappingServiceModel.CostPerUnit;

            _apparelProDbContext.Update(orderItemFeatureDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }

        private async Task ValidateFeatureCodesExistAsync(string? feature1Type, string? feature2Type, string? feature3Type, string? feature4Type)
        {
            var featureCodes = new[] { feature1Type, feature2Type, feature3Type, feature4Type }
                .Where(code => !string.IsNullOrEmpty(code))
                .Distinct()
                .ToList();

            if (featureCodes.Count == 0)
            {
                return;
            }

            var existingCount = await _apparelProDbContext.ItemFeatures
                .Where(itemFeature => featureCodes.Contains(itemFeature.FeatureCode))
                .CountAsync();

            if (existingCount != featureCodes.Count)
            {
                throw new InvalidOperationException("One or more Feature codes do not exist in the Item Feature master.");
            }
        }

        private async Task<OrderItemFeatureMappingServiceModel> MapToServiceModelWithNamesAsync(OrderItemFeature orderItemFeatureDbModel)
        {
            var serviceModel = _mapper.Map<OrderItemFeatureMappingServiceModel>(orderItemFeatureDbModel);

            var featureCodes = new[] { serviceModel.Feature1Type, serviceModel.Feature2Type, serviceModel.Feature3Type, serviceModel.Feature4Type }
                .Where(code => !string.IsNullOrEmpty(code))
                .Distinct()
                .ToList();

            if (featureCodes.Count > 0)
            {
                var featureNames = await _apparelProDbContext.ItemFeatures
                    .Where(itemFeature => featureCodes.Contains(itemFeature.FeatureCode))
                    .ToDictionaryAsync(itemFeature => itemFeature.FeatureCode, itemFeature => itemFeature.Description);

                serviceModel.Feature1Name = !string.IsNullOrEmpty(serviceModel.Feature1Type) && featureNames.TryGetValue(serviceModel.Feature1Type, out var f1) ? f1 : null;
                serviceModel.Feature2Name = !string.IsNullOrEmpty(serviceModel.Feature2Type) && featureNames.TryGetValue(serviceModel.Feature2Type, out var f2) ? f2 : null;
                serviceModel.Feature3Name = !string.IsNullOrEmpty(serviceModel.Feature3Type) && featureNames.TryGetValue(serviceModel.Feature3Type, out var f3) ? f3 : null;
                serviceModel.Feature4Name = !string.IsNullOrEmpty(serviceModel.Feature4Type) && featureNames.TryGetValue(serviceModel.Feature4Type, out var f4) ? f4 : null;
            }

            return serviceModel;
        }
    }
}
