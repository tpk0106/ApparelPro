using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IStyleComponentBreakdownService;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/style-component-breakdown")]
    [ApiController]
    public class StyleComponentBreakdownController : ControllerBase
    {
        private readonly IStyleComponentBreakdownService _styleComponentBreakdownService;
        private readonly IMapper _mapper;

        public StyleComponentBreakdownController(
            IStyleComponentBreakdownService styleComponentBreakdownService, IMapper mapper)
        {
            _styleComponentBreakdownService = styleComponentBreakdownService;
            _mapper = mapper;
        }

        [HttpGet("by-style")]
        [Authorize(Policy = "style-component-breakdown-view")]
        [ProducesResponseType(typeof(List<StyleComponentBreakdownAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetBreakdownByStyleAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            var serviceModels = await _styleComponentBreakdownService
                .GetBreakdownByStyleAsync(buyerCode, order, typeCode, styleCode);
            return Ok(_mapper.Map<List<StyleComponentBreakdownAPIModel>>(serviceModels));
        }

        [HttpPost("bulk-save")]
        [Authorize(Policy = "style-component-breakdown-manage")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        public async Task<IActionResult> BulkSaveAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode,
            [FromBody] List<CreateStyleComponentBreakdownAPIModel> payload)
        {
            if (payload == null) return BadRequest("Component breakdown payload cannot be empty.");

            var serviceModels = _mapper.Map<List<CreateStyleComponentBreakdownServiceModel>>(payload);
            await _styleComponentBreakdownService.BulkSaveComponentBreakdownAsync(
                buyerCode, order, typeCode, styleCode, serviceModels);

            return Ok(new { Message = "Component breakdown saved successfully." });
        }
    }
}
