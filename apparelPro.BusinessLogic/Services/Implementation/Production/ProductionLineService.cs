using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class ProductionLineService : IProductionLineService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public ProductionLineService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<ProductionLineServiceModel>> GetProductionLinesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<ProductionLine> query = _apparelProDbContext.ProductionLines.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(ProductionLine));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<ProductionLineServiceModel>>(pageResult);

            return new PaginationResult<ProductionLineServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<ProductionLineServiceModel?> GetProductionLineByLineCodeAsync(string lineCode)
        {
            var entity = await _apparelProDbContext.ProductionLines
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LineCode == lineCode);
            return entity == null ? null : _mapper.Map<ProductionLineServiceModel>(entity);
        }

        public async Task<ProductionLineServiceModel> AddProductionLineAsync(CreateProductionLineServiceModel createServiceModel)
        {
            var entity = _mapper.Map<ProductionLine>(createServiceModel);
            _apparelProDbContext.ProductionLines.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<ProductionLineServiceModel>(entity);
        }

        public async Task UpdateProductionLineAsync(UpdateProductionLineServiceModel updateServiceModel)
        {
            var entity = await _apparelProDbContext.ProductionLines
                .FirstOrDefaultAsync(l => l.LineCode == updateServiceModel.LineCode)
                ?? throw new KeyNotFoundException($"Production line '{updateServiceModel.LineCode}' was not found.");

            entity.Description = updateServiceModel.Description;
            entity.NumberOfMachines = updateServiceModel.NumberOfMachines;
            entity.CurrencyCode = updateServiceModel.CurrencyCode;
            entity.LineCostPerDay = updateServiceModel.LineCostPerDay;
            entity.MinimumProductionPerOrder = updateServiceModel.MinimumProductionPerOrder;
            entity.UnitCode = updateServiceModel.UnitCode;
            entity.NextAllocationDate = updateServiceModel.NextAllocationDate;
            entity.EstimatedNextAllocationDate = updateServiceModel.EstimatedNextAllocationDate;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteProductionLineAsync(string lineCode)
        {
            var entity = await _apparelProDbContext.ProductionLines.FirstOrDefaultAsync(l => l.LineCode == lineCode);
            if (entity == null) return;
            _apparelProDbContext.ProductionLines.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
