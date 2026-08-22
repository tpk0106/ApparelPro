using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionEntryService;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/daily-production-entry")]
    [ApiController]
    public class DailyProductionEntryController : ControllerBase
    {
        private readonly IDailyProductionEntryService _dailyProductionEntryService;
        private readonly IMapper _mapper;

        public DailyProductionEntryController(
            IDailyProductionEntryService dailyProductionEntryService, IMapper mapper)
        {
            _dailyProductionEntryService = dailyProductionEntryService;
            _mapper = mapper;
        }

        [HttpGet("by-date")]
        [Authorize(Policy = "daily-production-entry-view")]
        [ProducesResponseType(typeof(List<DailyProductionEntryAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetByDateAsync(
            [FromQuery] DateOnly date, [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode, [FromQuery] string lineCode)
        {
            var serviceModels = await _dailyProductionEntryService.GetByDateAsync(
                date, buyerCode, order, typeCode, styleCode, lineCode);
            return Ok(_mapper.Map<List<DailyProductionEntryAPIModel>>(serviceModels));
        }

        [HttpPost("bulk-save")]
        [Authorize(Policy = "daily-production-entry-manage")]
        [ProducesResponseType(typeof(List<DailyProductionEntryAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> BulkSaveAsync(
            [FromQuery] DateOnly date, [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode, [FromQuery] string lineCode,
            [FromBody] List<CreateDailyProductionEntryAPIModel> payload)
        {
            if (payload == null) return BadRequest("Daily production entry payload cannot be empty.");

            var serviceModels = _mapper.Map<List<CreateDailyProductionEntryServiceModel>>(payload);

            try
            {
                var result = await _dailyProductionEntryService.BulkSaveAsync(
                    date, buyerCode, order, typeCode, styleCode, lineCode, serviceModels);
                return Ok(_mapper.Map<List<DailyProductionEntryAPIModel>>(result));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
