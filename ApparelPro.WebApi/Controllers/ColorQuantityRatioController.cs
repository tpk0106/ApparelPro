using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorQuantityRatioService;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/colorQuantityRatio")]
    [ApiController]
    public class ColorQuantityRatioController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IColorQuantityRatioService _colorQuantityRatioService;

        public ColorQuantityRatioController(IMapper mapper, IColorQuantityRatioService colorQuantityRatioService)
        {
            _mapper = mapper;
            _colorQuantityRatioService = colorQuantityRatioService;
        }

        [HttpGet("by-style")]
        [Authorize(Policy = "color-size-breakdown")]
        [ProducesResponseType(typeof(List<ColorQuantityRatioAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetByStyleAsync(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            var serviceModels = await _colorQuantityRatioService.GetByStyleAsync(buyerCode, order, typeCode, styleCode);
            var mappedResult = _mapper.Map<List<ColorQuantityRatioAPIModel>>(serviceModels);
            return Ok(mappedResult);
        }

        [HttpPost("bulk-save")]
        [Authorize(Policy = "color-size-breakdown-bulk-save")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        public async Task<IActionResult> BulkSaveAsync(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode,
            [FromBody] List<CreateColorQuantityRatioAPIModel> payload)
        {
            if (payload == null) return BadRequest("Colour allocation payload cannot be empty.");

            var serviceModels = _mapper.Map<List<CreateColorQuantityRatioServiceModel>>(payload);

            try
            {
                await _colorQuantityRatioService.BulkSaveColorQuantityRatiosAsync(buyerCode, order, typeCode, styleCode, serviceModels);
                return Ok(new { Message = "Colour allocation synced with SQL Server successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("set-color-ratio-mode")]
        [Authorize(Policy = "color-size-breakdown-bulk-save")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        public async Task<IActionResult> SetColorRatioModeAsync([FromBody] SetRatioModeAPIModel model)
        {
            try
            {
                await _colorQuantityRatioService.SetColorRatioModeAsync(model.BuyerCode, model.Order, model.TypeCode, model.StyleCode, model.Mode);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("set-size-ratio-mode")]
        [Authorize(Policy = "color-size-breakdown-bulk-save")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        public async Task<IActionResult> SetSizeRatioModeAsync([FromBody] SetRatioModeAPIModel model)
        {
            try
            {
                await _colorQuantityRatioService.SetSizeRatioModeAsync(model.BuyerCode, model.Order, model.TypeCode, model.StyleCode, model.Mode);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
