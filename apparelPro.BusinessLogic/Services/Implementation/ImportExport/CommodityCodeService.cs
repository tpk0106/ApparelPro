using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICommodityCodeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class CommodityCodeService : ICommodityCodeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public CommodityCodeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<CommodityCodeServiceModel> AddCommodityCodeAsync(CreateCommodityCodeServiceModel createCommodityCodeServiceModel)
        {
            try
            {
                var commodityCodeDbModel = _mapper.Map<CommodityCode>(createCommodityCodeServiceModel);
                _apparelProDbContext.CommodityCodes.Add(commodityCodeDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<CommodityCodeServiceModel>(commodityCodeDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Commodity Code already exists");
            }
        }

        public async Task DeleteCommodityCodeAsync(string code)
        {
            try
            {
                var commodityCodeDbModel = await _apparelProDbContext.CommodityCodes
                    .Where(commodityCode => commodityCode.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.CommodityCodes.Remove(commodityCodeDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<CommodityCodeServiceModel>> GetCommodityCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<CommodityCode> commodityCodePagination = _apparelProDbContext.CommodityCodes.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(CommodityCode));
                commodityCodePagination = commodityCodePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await commodityCodePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                commodityCodePagination = commodityCodePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            commodityCodePagination = commodityCodePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbCommodityCodes = await commodityCodePagination.ToListAsync();
            var commodityCodeServiceModels = _mapper.Map<IList<CommodityCodeServiceModel>>(filteredDbCommodityCodes);

            return new PaginationResult<CommodityCodeServiceModel>(pageSize, pageNumber, counter, commodityCodeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateCommodityCodeAsync(UpdateCommodityCodeServiceModel updateCommodityCodeServiceModel)
        {
            try
            {
                var commodityCodeDbModel = await _apparelProDbContext.CommodityCodes
                    .Where(commodityCode => commodityCode.Code == updateCommodityCodeServiceModel.Code)
                    .FirstOrDefaultAsync();
                if (commodityCodeDbModel != null)
                {
                    commodityCodeDbModel.Description = updateCommodityCodeServiceModel.Description;
                    _apparelProDbContext.CommodityCodes.Update(commodityCodeDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<CommodityCodeServiceModel> GetCommodityCodeByCodeAsync(string code)
        {
            var commodityCodeDbModel = await _apparelProDbContext.CommodityCodes.Where(commodityCode => commodityCode.Code == code)
                .FirstOrDefaultAsync();
            var commodityCodeServiceModel = _mapper.Map<CommodityCodeServiceModel>(commodityCodeDbModel);
            return commodityCodeServiceModel;
        }
    }
}
