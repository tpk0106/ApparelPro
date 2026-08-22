using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IStyleOperationBreakdownService;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/style-operation-breakdown")]
    [ApiController]
    public class StyleOperationBreakdownController : ControllerBase
    {
        private readonly IStyleOperationBreakdownService _styleOperationBreakdownService;
        private readonly IMapper _mapper;

        public StyleOperationBreakdownController(
            IStyleOperationBreakdownService styleOperationBreakdownService, IMapper mapper)
        {
            _styleOperationBreakdownService = styleOperationBreakdownService;
            _mapper = mapper;
        }

        [HttpGet("by-style")]
        [Authorize(Policy = "style-operation-breakdown-view")]
        [ProducesResponseType(typeof(List<StyleOperationBreakdownAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetBreakdownByStyleAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            var serviceModels = await _styleOperationBreakdownService
                .GetBreakdownByStyleAsync(buyerCode, order, typeCode, styleCode);
            return Ok(_mapper.Map<List<StyleOperationBreakdownAPIModel>>(serviceModels));
        }

        [HttpPost("seed-from-template")]
        [Authorize(Policy = "style-operation-breakdown-manage")]
        [ProducesResponseType(typeof(List<StyleOperationBreakdownAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> SeedFromTemplateAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode,
            [FromQuery] int componentSequence, [FromQuery] string componentCode)
        {
            var serviceModels = await _styleOperationBreakdownService.SeedFromTemplateAsync(
                buyerCode, order, typeCode, styleCode, componentSequence, componentCode);
            return Ok(_mapper.Map<List<StyleOperationBreakdownAPIModel>>(serviceModels));
        }

        [HttpPost("bulk-save")]
        [Authorize(Policy = "style-operation-breakdown-manage")]
        [ProducesResponseType(typeof(StyleOperationBreakdownSaveResultAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> BulkSaveAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode,
            [FromBody] List<CreateStyleOperationBreakdownAPIModel> payload)
        {
            if (payload == null) return BadRequest("Operation breakdown payload cannot be empty.");

            var serviceModels = _mapper.Map<List<CreateStyleOperationBreakdownServiceModel>>(payload);
            var result = await _styleOperationBreakdownService.BulkSaveAndRecalculateAsync(
                buyerCode, order, typeCode, styleCode, serviceModels);

            return Ok(_mapper.Map<StyleOperationBreakdownSaveResultAPIModel>(result));
        }
    }
}
