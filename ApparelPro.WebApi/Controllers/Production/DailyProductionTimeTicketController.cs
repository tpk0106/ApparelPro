using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionTimeTicketService;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/daily-production-time-ticket")]
    [ApiController]
    public class DailyProductionTimeTicketController : ControllerBase
    {
        private readonly IDailyProductionTimeTicketService _dailyProductionTimeTicketService;
        private readonly IMapper _mapper;

        public DailyProductionTimeTicketController(
            IDailyProductionTimeTicketService dailyProductionTimeTicketService, IMapper mapper)
        {
            _dailyProductionTimeTicketService = dailyProductionTimeTicketService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "daily-production-time-ticket-view")]
        [ProducesResponseType(typeof(DailyProductionTimeTicketAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetByTicketAsync(
            [FromQuery] DateOnly date, [FromQuery] string lineCode,
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            var serviceModel = await _dailyProductionTimeTicketService.GetByTicketAsync(
                date, lineCode, buyerCode, order, typeCode, styleCode);
            return Ok(_mapper.Map<DailyProductionTimeTicketAPIModel>(serviceModel));
        }

        [HttpPost("bulk-save")]
        [Authorize(Policy = "daily-production-time-ticket-manage")]
        [ProducesResponseType(typeof(DailyProductionTimeTicketAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> BulkSaveAsync(
            [FromQuery] DateOnly date, [FromQuery] string lineCode,
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode,
            [FromBody] List<CreateDailyProductionTimeTicketEntryAPIModel> payload)
        {
            if (payload == null) return BadRequest("Time ticket payload cannot be empty.");

            var serviceModels = _mapper.Map<List<CreateDailyProductionTimeTicketEntryServiceModel>>(payload);

            try
            {
                var result = await _dailyProductionTimeTicketService.BulkSaveAsync(
                    date, lineCode, buyerCode, order, typeCode, styleCode, serviceModels);
                return Ok(_mapper.Map<DailyProductionTimeTicketAPIModel>(result));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
