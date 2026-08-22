using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.IGarmentComponentService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class GarmentComponentService : IGarmentComponentService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public GarmentComponentService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<GarmentComponentServiceModel>> GetGarmentComponentsAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<GarmentComponent> query = _apparelProDbContext.GarmentComponents.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(GarmentComponent));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<GarmentComponentServiceModel>>(pageResult);

            return new PaginationResult<GarmentComponentServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<GarmentComponentServiceModel?> GetGarmentComponentByComponentCodeAsync(string componentCode)
        {
            var entity = await _apparelProDbContext.GarmentComponents
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ComponentCode == componentCode);
            return entity == null ? null : _mapper.Map<GarmentComponentServiceModel>(entity);
        }

        public async Task<GarmentComponentServiceModel> AddGarmentComponentAsync(CreateGarmentComponentServiceModel createServiceModel)
        {
            var entity = _mapper.Map<GarmentComponent>(createServiceModel);
            _apparelProDbContext.GarmentComponents.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<GarmentComponentServiceModel>(entity);
        }

        public async Task UpdateGarmentComponentAsync(UpdateGarmentComponentServiceModel updateServiceModel)
        {
            var entity = await _apparelProDbContext.GarmentComponents
                .FirstOrDefaultAsync(c => c.ComponentCode == updateServiceModel.ComponentCode)
                ?? throw new KeyNotFoundException($"Garment component '{updateServiceModel.ComponentCode}' was not found.");

            entity.Description = updateServiceModel.Description;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteGarmentComponentAsync(string componentCode)
        {
            var entity = await _apparelProDbContext.GarmentComponents.FirstOrDefaultAsync(c => c.ComponentCode == componentCode);
            if (entity == null) return;
            _apparelProDbContext.GarmentComponents.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
