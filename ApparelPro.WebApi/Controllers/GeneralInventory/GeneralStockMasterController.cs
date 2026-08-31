using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.GeneralInventory
{
    // Item/Feature picker reuses the existing api/material-consumption/catalog and
    // api/material-consumption/feature-headers endpoints - General Inventory items are
    // drawn from the same StockItems/OrderItemFeatures/ItemFeatures catalog Order
    // Management's Material Consumption screen already uses, not a separate one.
    [Route("api/general-inventory-stock-master")]
    [ApiController]
    [Authorize(Policy = "general-stock-master")]
    public class GeneralStockMasterController : ControllerBase
    {
        private readonly IGeneralStockMasterService _generalStockMasterService;
        private readonly IMapper _mapper;

        public GeneralStockMasterController(IGeneralStockMasterService generalStockMasterService, IMapper mapper)
        {
            _generalStockMasterService = generalStockMasterService;
            _mapper = mapper;
        }

        // POST: api/general-inventory-stock-master/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitStockMaster([FromBody] GeneralStockMasterEntryAPIModel entryAPIModel)
        {
            if (entryAPIModel == null)
                return BadRequest("Validation Failure: Missing entry.");

            var entryServiceModel = _mapper.Map<GeneralStockMasterEntryServiceModel>(entryAPIModel);

            try
            {
                var success = await _generalStockMasterService.CommitGeneralStockMasterAsync(entryServiceModel);
                return Ok(new { Success = success, Message = "Stock Master entry saved successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transaction processing failed: {ex.Message}" });
            }
        }

        // PUT: api/general-inventory-stock-master/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateStockMaster([FromBody] GeneralStockMasterUpdateAPIModel updateAPIModel)
        {
            if (updateAPIModel == null)
                return BadRequest("Validation Failure: Missing entry.");

            var updateServiceModel = _mapper.Map<GeneralStockMasterUpdateServiceModel>(updateAPIModel);

            try
            {
                var success = await _generalStockMasterService.UpdateGeneralStockMasterAsync(updateServiceModel);
                return Ok(new { Success = success, Message = "Stock Master entry updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transaction processing failed: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-stock-master/by-store?storeCode=M-S
        [HttpGet("by-store")]
        public async Task<IActionResult> GetByStore([FromQuery] string storeCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");

            try
            {
                var result = await _generalStockMasterService.GetGeneralStockMastersByStoreAsync(storeCode);
                return Ok(_mapper.Map<List<GeneralStockMasterRowAPIModel>>(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Stock Master items: {ex.Message}" });
            }
        }

        // DELETE: api/general-inventory-stock-master?storeCode=M-S&itemCode=...
        [HttpDelete]
        public async Task<IActionResult> DeleteStockMaster([FromQuery] string storeCode, [FromQuery] string itemCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode) || string.IsNullOrWhiteSpace(itemCode))
                return BadRequest("Parameters 'storeCode' and 'itemCode' are required.");

            try
            {
                await _generalStockMasterService.DeleteGeneralStockMasterAsync(storeCode, itemCode);
                return Ok(new { Success = true, Message = "Stock Master item deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to delete Stock Master item: {ex.Message}" });
            }
        }
    }
}
