using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IStockService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class StockService : IStockService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public StockService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
        }

        public async Task<StockServiceModel> AddStockAsync(CreateStockServiceModel createStockServiceModel)
        {
            try
            {
                var stockDbModel = _mapper.Map<Stock>(createStockServiceModel);
                _apparelProDbContext.Stocks.Add(stockDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<StockServiceModel>(stockDbModel);
            }
            catch (Exception)
            {
                throw new Exception("Stock Reference Code already exists.");
            }
        }

        public async Task DeleteStockAsync(string stockCode)
        {
            var stockDbModel = await _apparelProDbContext.Stocks
                .Where(stock => stock.StockCode == stockCode)
                .FirstOrDefaultAsync();
            try
            {
                _apparelProDbContext.Stocks.Remove(stockDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new Exception("Cannot delete - this Stock Code is already in use by one or more catalog Items.");
            }
        }

        public async Task<bool> DoesStockExistAsync(string stockCode)
        {
            return await _apparelProDbContext.Stocks.AnyAsync(stock => stock.StockCode == stockCode);
        }

        public async Task<StockServiceModel?> GetStockByCodeAsync(string stockCode)
        {
            var stockDbModel = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .Where(stock => stock.StockCode == stockCode)
                .FirstOrDefaultAsync();
            return _mapper.Map<StockServiceModel>(stockDbModel);
        }

        public async Task<PaginationResult<StockServiceModel>> GetStocksAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Stock> stocksPagination = _apparelProDbContext.Stocks.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Stock));
                stocksPagination = stocksPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = await stocksPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                stocksPagination = stocksPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            else
            {
                stocksPagination = stocksPagination.OrderBy(stock => stock.StockCode);
            }

            stocksPagination = stocksPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbStocks = await stocksPagination.ToListAsync();
            var stockServiceModels = _mapper.Map<IList<StockServiceModel>>(filteredDbStocks);

            return new PaginationResult<StockServiceModel>(pageSize, pageNumber, counter, stockServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateStockAsync(UpdateStockServiceModel updateStockServiceModel)
        {
            var stockDbModel = await _apparelProDbContext.Stocks
                .Where(stock => stock.StockCode == updateStockServiceModel.StockCode)
                .FirstOrDefaultAsync();

            stockDbModel!.Description = updateStockServiceModel.Description;

            _apparelProDbContext.Update(stockDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
