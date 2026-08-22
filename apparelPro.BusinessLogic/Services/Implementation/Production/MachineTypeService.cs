using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.IMachineTypeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class MachineTypeService : IMachineTypeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public MachineTypeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<MachineTypeServiceModel>> GetMachineTypesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<MachineType> query = _apparelProDbContext.MachineTypes.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(MachineType));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<MachineTypeServiceModel>>(pageResult);

            return new PaginationResult<MachineTypeServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<MachineTypeServiceModel?> GetMachineTypeByCodeAsync(string code)
        {
            var entity = await _apparelProDbContext.MachineTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Code == code);
            return entity == null ? null : _mapper.Map<MachineTypeServiceModel>(entity);
        }

        public async Task<MachineTypeServiceModel> AddMachineTypeAsync(CreateMachineTypeServiceModel createServiceModel)
        {
            var entity = _mapper.Map<MachineType>(createServiceModel);
            _apparelProDbContext.MachineTypes.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<MachineTypeServiceModel>(entity);
        }

        public async Task UpdateMachineTypeAsync(UpdateMachineTypeServiceModel updateServiceModel)
        {
            var entity = await _apparelProDbContext.MachineTypes
                .FirstOrDefaultAsync(m => m.Code == updateServiceModel.Code)
                ?? throw new KeyNotFoundException($"Machine type '{updateServiceModel.Code}' was not found.");

            entity.Description = updateServiceModel.Description;
            entity.IsManual = updateServiceModel.IsManual;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteMachineTypeAsync(string code)
        {
            var entity = await _apparelProDbContext.MachineTypes.FirstOrDefaultAsync(m => m.Code == code);
            if (entity == null) return;
            _apparelProDbContext.MachineTypes.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
