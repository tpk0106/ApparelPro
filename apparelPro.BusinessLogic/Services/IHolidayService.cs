using apparelPro.BusinessLogic.Services.Models.Production.IHolidayService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IHolidayService
    {
        Task<PaginationResult<HolidayServiceModel>> GetHolidaysAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<HashSet<DateOnly>> GetAllHolidayDatesAsync();
        Task<HolidayServiceModel> AddHolidayAsync(CreateHolidayServiceModel createServiceModel);
        Task DeleteHolidayAsync(DateOnly date);
    }
}
