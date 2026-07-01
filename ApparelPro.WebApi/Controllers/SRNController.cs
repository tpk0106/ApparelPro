using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/orderwise-inventory-srn")]
    [ApiController]
    public class SRNController : ControllerBase
    {
        private readonly IStoresRequisitionService _storesRequisitionService;
        private readonly IMapper _mapper;

        public SRNController(IStoresRequisitionService  storesRequisitionService, IMapper mapper)
        {
            _storesRequisitionService = storesRequisitionService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-srn/verify-stock?buyerCode=2&order=1017-18&storeCode=STR&itemCode=02BT&targetUnit=PCS
        [HttpGet("verify-stock")]
        public async Task<IActionResult> VerifyStock(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] string storeCode,
            [FromQuery] string itemCode,
            [FromQuery] string targetUnit)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(storeCode) || string.IsNullOrEmpty(itemCode))
            {
                return BadRequest("Tracking constraint parameters 'order', 'storeCode', and 'itemCode' cannot be empty.");
            }

            try
            {
                var availability = await _storesRequisitionService.VerifyStockItemAvailabilityAsync(
                    buyerCode, order, storeCode, itemCode, targetUnit
                );

                if (availability == null)
                    return NotFound("The requested material item could not be discovered inside inventory stocks ledger pools.");

                return Ok(availability);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to read live item balance metrics: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-srn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitRequisition([FromBody] SRNAPIModel srnAPIModel)
        {
            if (srnAPIModel == null)
                return BadRequest("The inbound spreadsheet allocation model payload cannot be empty.");

            if (srnAPIModel.Header == null || srnAPIModel.Lines == null || srnAPIModel.Lines.Count == 0)
            {
                return BadRequest("Validation Failure: Missing critical document headers or line items data.");
            }

            // Extract the authenticated username context cleanly from the API session token headers identity
            string activeUsername = User.Identity?.Name ?? "STORES CLERK";
            var srnServiceModel = _mapper.Map<SRNServiceModel>(srnAPIModel);

            try
            {
                var success = await _storesRequisitionService.CommitStoresRequisitionNoteAsync(srnServiceModel.Header, srnServiceModel.Lines, activeUsername);

                // Return a descriptive tracking success payload to output inside frontend toasts
                return Ok(new
                {
                    Success = success,
                    Message = $"Stores Requisition Note '{srnAPIModel.Header.SrnNumber}' committed and inventory balances updated successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                // Gracefully intercepts deficit rejections and data clash exceptions
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transaction processing failed on the SQL server: {ex.Message}" });
            }
        }

        // GET: api/orderwise-inventory-srn/available-choices?buyerCode=2&order=1017-18
        [HttpGet("available-choices")]
        public async Task<IActionResult> GetAvailableChoices([FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] string storeCode)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(storeCode))
                return BadRequest("Parameters 'order' and 'storeCode' are required.");

            try
            {
                var choices = await _storesRequisitionService.GetAvailableStockChoicesAsync(buyerCode, order, storeCode);
                return Ok(choices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to build stock search options: {ex.Message}" });
            }
        }

    }
}
