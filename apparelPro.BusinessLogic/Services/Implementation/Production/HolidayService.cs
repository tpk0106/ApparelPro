using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.IHolidayService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class HolidayService : IHolidayService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public HolidayService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<HolidayServiceModel>> GetHolidaysAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Holiday> query = _apparelProDbContext.Holidays.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(Holiday));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            query = sortColumn != null
                ? query.OrderBy(string.Format("{0} {1}", sortColumn,
                    !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC"))
                : query.OrderBy(h => h.Date);

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<HolidayServiceModel>>(pageResult);

            return new PaginationResult<HolidayServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<HashSet<DateOnly>> GetAllHolidayDatesAsync()
        {
            var dates = await _apparelProDbContext.Holidays.AsNoTracking().Select(h => h.Date).ToListAsync();
            return dates.ToHashSet();
        }

        public async Task<HolidayServiceModel> AddHolidayAsync(CreateHolidayServiceModel createServiceModel)
        {
            var entity = _mapper.Map<Holiday>(createServiceModel);
            _apparelProDbContext.Holidays.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<HolidayServiceModel>(entity);
        }

        public async Task DeleteHolidayAsync(DateOnly date)
        {
            var entity = await _apparelProDbContext.Holidays.FirstOrDefaultAsync(h => h.Date == date);
            if (entity == null) return;
            _apparelProDbContext.Holidays.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
