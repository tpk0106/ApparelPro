using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IHolidayService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/holiday")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _holidayService;
        private readonly IMapper _mapper;

        public HolidayController(IHolidayService holidayService, IMapper mapper)
        {
            _holidayService = holidayService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "holiday-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<HolidayAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetHolidaysAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _holidayService.GetHolidaysAsync(
                pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            return Ok(_mapper.Map<PaginationAPIModel<HolidayAPIModel>>(serviceModels));
        }

        [HttpPost]
        [Authorize(Policy = "holiday-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddHolidayAsync([FromBody] CreateHolidayAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateHolidayServiceModel>(createAPIModel);
            await _holidayService.AddHolidayAsync(createServiceModel);
            return StatusCode(HttpStatusCodes.Created);
        }

        [HttpDelete("{date}")]
        [Authorize(Policy = "holiday-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> DeleteHolidayAsync(DateOnly date)
        {
            await _holidayService.DeleteHolidayAsync(date);
            return NoContent();
        }
    }
}
