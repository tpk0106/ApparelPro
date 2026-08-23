using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Production Control -> End of Production Confirmation - migrated
    // from PR_ENDPR.PRG.
    [Route("api/end-of-production-confirmation")]
    [ApiController]
    public class EndOfProductionConfirmationController : ControllerBase
    {
        private readonly IEndOfProductionConfirmationService _endOfProductionConfirmationService;
        private readonly IMapper _mapper;

        public EndOfProductionConfirmationController(
            IEndOfProductionConfirmationService endOfProductionConfirmationService, IMapper mapper)
        {
            _endOfProductionConfirmationService = endOfProductionConfirmationService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "end-of-production-confirmation-view")]
        [ProducesResponseType(typeof(EndOfProductionStatusAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            try
            {
                var status = await _endOfProductionConfirmationService.GetStatusAsync(buyerCode, order, typeCode, styleCode);
                return Ok(_mapper.Map<EndOfProductionStatusAPIModel>(status));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpPost("confirm")]
        [Authorize(Policy = "end-of-production-confirmation-manage")]
        [ProducesResponseType(typeof(EndOfProductionStatusAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> ConfirmAsync([FromBody] ConfirmEndOfProductionAPIModel request)
        {
            try
            {
                var status = await _endOfProductionConfirmationService.ConfirmAsync(
                    request.BuyerCode, request.Order, request.TypeCode, request.StyleCode, request.EndDate);
                return Ok(_mapper.Map<EndOfProductionStatusAPIModel>(status));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
