using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeItemsService;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    // Replicates OD_ITM1.PRG (Update) / OD_ITM2.PRG (List) - Reference Files >
    // B. Order Management > G. Type / Item in legacy (RF_MENU.PRG). Garment Type wise
    // default Item/Accessory requirement breakdown (od_tpdtr.dbf).
    [Route("api/garment-type-items")]
    [ApiController]
    public class GarmentTypeItemsController : ControllerBase
    {
        private readonly IGarmentTypeItemsService _garmentTypeItemsService;
        private readonly IMapper _mapper;

        public GarmentTypeItemsController(IGarmentTypeItemsService garmentTypeItemsService, IMapper mapper)
        {
            _garmentTypeItemsService = garmentTypeItemsService ?? throw new ArgumentNullException(nameof(garmentTypeItemsService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("list")]
        [Authorize(Policy = "garment-type-item-view")]
        [ProducesResponseType(typeof(List<GarmentTypeItemAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetGarmentTypeItemsAsync([FromQuery] int garmentTypeId)
        {
            var result = await _garmentTypeItemsService.GetGarmentTypeItemsAsync(garmentTypeId);
            return Ok(_mapper.Map<List<GarmentTypeItemAPIModel>>(result));
        }

        [HttpPost]
        [Authorize(Policy = "garment-type-item-manage")]
        [ProducesResponseType(typeof(GarmentTypeItemAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveGarmentTypeItemAsync([FromBody] SaveGarmentTypeItemAPIModel request)
        {
            if (request == null) return BadRequest("Entry request data payload cannot be empty.");
            try
            {
                var serviceModel = _mapper.Map<SaveGarmentTypeItemServiceModel>(request);
                var result = await _garmentTypeItemsService.SaveGarmentTypeItemAsync(serviceModel);
                return Ok(_mapper.Map<GarmentTypeItemAPIModel>(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to save Garment Type Item entry: {ex.Message}" });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "garment-type-item-manage")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteGarmentTypeItemAsync(
            [FromQuery] int garmentTypeId, [FromQuery] string stockCode, [FromQuery] string itemCode)
        {
            try
            {
                var success = await _garmentTypeItemsService.DeleteGarmentTypeItemAsync(garmentTypeId, stockCode, itemCode);
                if (!success) return NotFound(new { Message = "Item requirement line not found for this Garment Type." });
                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Deletion Failed: {ex.Message}" });
            }
        }
    }
}
