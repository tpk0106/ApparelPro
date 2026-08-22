using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.INonProductiveHourCodeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class NonProductiveHourCodeService : INonProductiveHourCodeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public NonProductiveHourCodeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<NonProductiveHourCodeServiceModel>> GetNonProductiveHourCodesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<NonProductiveHourCode> query = _apparelProDbContext.NonProductiveHourCodes.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(NonProductiveHourCode));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<NonProductiveHourCodeServiceModel>>(pageResult);

            return new PaginationResult<NonProductiveHourCodeServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<NonProductiveHourCodeServiceModel?> GetNonProductiveHourCodeByCodeAsync(string code)
        {
            var entity = await _apparelProDbContext.NonProductiveHourCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Code == code);
            return entity == null ? null : _mapper.Map<NonProductiveHourCodeServiceModel>(entity);
        }

        public async Task<NonProductiveHourCodeServiceModel> AddNonProductiveHourCodeAsync(CreateNonProductiveHourCodeServiceModel createServiceModel)
        {
            var entity = _mapper.Map<NonProductiveHourCode>(createServiceModel);
            _apparelProDbContext.NonProductiveHourCodes.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<NonProductiveHourCodeServiceModel>(entity);
        }

        public async Task UpdateNonProductiveHourCodeAsync(UpdateNonProductiveHourCodeServiceModel updateServiceModel)
        {
            var entity = await _apparelProDbContext.NonProductiveHourCodes
                .FirstOrDefaultAsync(n => n.Code == updateServiceModel.Code)
                ?? throw new KeyNotFoundException($"Non-productive hour code '{updateServiceModel.Code}' was not found.");

            entity.Description = updateServiceModel.Description;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteNonProductiveHourCodeAsync(string code)
        {
            var entity = await _apparelProDbContext.NonProductiveHourCodes.FirstOrDefaultAsync(n => n.Code == code);
            if (entity == null) return;
            _apparelProDbContext.NonProductiveHourCodes.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
