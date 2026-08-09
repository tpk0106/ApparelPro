using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemCatalogService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    // Order Items Catalog (od_itm) UI over the StockItems table/StockItem entity -
    // NOT the older OrderItems table. StockItems is the catalog every live
    // transactional screen actually validates/looks descriptions up against
    // (Material Consumption's auto-add-on-save, GarmentAdditionalCostService,
    // OrderItemFeatureService, and the GRN/DGN/GTN/SAN/SRN/Supplier Return note
    // services), so this admin screen has to read/write the same table those
    // depend on rather than the separate, mostly-dormant OrderItems table.
    public class OrderItemCatalogService : IOrderItemCatalogService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public OrderItemCatalogService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
        }

        public async Task<OrderItemCatalogServiceModel> AddOrderItemCatalogAsync(CreateOrderItemCatalogServiceModel createOrderItemCatalogServiceModel)
        {
            var stockExists = await _apparelProDbContext.Stocks
                .AnyAsync(stock => stock.StockCode == createOrderItemCatalogServiceModel.StockCode);
            if (!stockExists)
            {
                throw new Exception("Invalid Stock Code - it must exist in Stock Reference first.");
            }

            try
            {
                var stockItemDbModel = _mapper.Map<StockItem>(createOrderItemCatalogServiceModel);
                _apparelProDbContext.StockItems.Add(stockItemDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return await MapWithStockDescriptionAsync(stockItemDbModel);
            }
            catch (Exception)
            {
                throw new Exception("This Stock/Item Code combination already exists in the catalog.");
            }
        }

        public async Task DeleteOrderItemCatalogAsync(string stockCode, string itemCode)
        {
            var stockItemDbModel = await _apparelProDbContext.StockItems
                .Where(stockItem => stockItem.StockCode == stockCode && stockItem.ItemCode == itemCode)
                .FirstOrDefaultAsync();
            try
            {
                _apparelProDbContext.StockItems.Remove(stockItemDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new Exception("Cannot delete - this catalog Item is already in use elsewhere (e.g. Garment Type Item Requirements or Material Consumption).");
            }
        }

        public async Task<bool> DoesOrderItemCatalogExistAsync(string stockCode, string itemCode)
        {
            return await _apparelProDbContext.StockItems
                .AnyAsync(stockItem => stockItem.StockCode == stockCode && stockItem.ItemCode == itemCode);
        }

        public async Task<OrderItemCatalogServiceModel?> GetOrderItemCatalogByCodeAsync(string stockCode, string itemCode)
        {
            var stockItemDbModel = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(stockItem => stockItem.StockCode == stockCode && stockItem.ItemCode == itemCode)
                .FirstOrDefaultAsync();
            if (stockItemDbModel == null)
            {
                return null;
            }
            return await MapWithStockDescriptionAsync(stockItemDbModel);
        }

        public async Task<PaginationResult<OrderItemCatalogServiceModel>> GetOrderItemCatalogAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<StockItem> stockItemsPagination = _apparelProDbContext.StockItems.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(StockItem));
                stockItemsPagination = stockItemsPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = await stockItemsPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                stockItemsPagination = stockItemsPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            else
            {
                stockItemsPagination = stockItemsPagination.OrderBy(stockItem => stockItem.StockCode).ThenBy(stockItem => stockItem.ItemCode);
            }

            stockItemsPagination = stockItemsPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbStockItems = await stockItemsPagination.ToListAsync();

            // Bulk-fetch Stock descriptions once (small reference table), same pattern
            // MaterialConsumptionService uses, instead of an N+1 lookup per row.
            var stockDescriptionsByCode = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .ToDictionaryAsync(stock => stock.StockCode, stock => stock.Description);

            var stockItemServiceModels = filteredDbStockItems.Select(stockItem => new OrderItemCatalogServiceModel
            {
                StockCode = stockItem.StockCode,
                StockDescription = stockDescriptionsByCode.TryGetValue(stockItem.StockCode, out var description) ? description : string.Empty,
                ItemCode = stockItem.ItemCode,
                Description = stockItem.Description,
            }).ToList();

            return new PaginationResult<OrderItemCatalogServiceModel>(pageSize, pageNumber, counter, stockItemServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateOrderItemCatalogAsync(UpdateOrderItemCatalogServiceModel updateOrderItemCatalogServiceModel)
        {
            var stockItemDbModel = await _apparelProDbContext.StockItems
                .Where(stockItem => stockItem.StockCode == updateOrderItemCatalogServiceModel.StockCode
                    && stockItem.ItemCode == updateOrderItemCatalogServiceModel.ItemCode)
                .FirstOrDefaultAsync();

            stockItemDbModel!.Description = updateOrderItemCatalogServiceModel.Description;

            _apparelProDbContext.Update(stockItemDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }

        private async Task<OrderItemCatalogServiceModel> MapWithStockDescriptionAsync(StockItem stockItemDbModel)
        {
            var stock = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .Where(s => s.StockCode == stockItemDbModel.StockCode)
                .FirstOrDefaultAsync();

            var serviceModel = _mapper.Map<OrderItemCatalogServiceModel>(stockItemDbModel);
            serviceModel.StockDescription = stock?.Description ?? string.Empty;
            return serviceModel;
        }
    }
}
