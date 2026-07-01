using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPartShipmentService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/part-shipment")]
    [ApiController]
    public class PartShipmentController : ControllerBase
    {
        private readonly IPartShipmentService _partShipmentService;

        public PartShipmentController(IPartShipmentService partShipmentService)
        {
            _partShipmentService = partShipmentService;
        }

        // 1. GET: api/part-shipment/style-summary?buyerCode=2&order=1017-18&typeCode=2&styleCode=M102
        [HttpGet("style-summary")]
        public async Task<IActionResult> GetStyleSummary(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(styleCode))
                return BadRequest("Tracking parameters 'order' and 'styleCode' cannot be empty.");

            try
            {
                var summary = await _partShipmentService.GetStyleShippingSummaryAsync(buyerCode, order, typeCode, styleCode);
                if (summary == null) return NotFound("The requested target style master profile could not be found.");
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to calculate style scheduling summaries: {ex.Message}" });
            }
        }

        // 2. GET: api/part-shipment/lines-ledger?buyerCode=2&order=1017-18&typeCode=2&styleCode=M102
        [HttpGet("lines-ledger")]
        public async Task<IActionResult> GetLinesLedger(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(styleCode))
                return BadRequest("Tracking reference parameters cannot be empty.");

            try
            {
                var partialLines = await _partShipmentService.GetPartShipmentsByStyleAsync(buyerCode, order, typeCode, styleCode);
                return Ok(partialLines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to retrieve partial shipping manifest ledger lines: {ex.Message}" });
            }
        }

        // 3. POST: api/part-shipment/save-line
        [HttpPost("save-line")]
        public async Task<IActionResult> SaveLine([FromBody] PartShipmentServiceModel request)
        {
            if (request == null) return BadRequest("The inbound manifest payload cannot be null.");
            try
            {
                var success = await _partShipmentService.SavePartShipmentLineAsync(request);
                return Ok(new { Success = success, Message = "Partial shipping manifest line processed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                // Catches explicit validation error blocks like "Quantity exceeds contract limit" or "Already Exported"
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transaction processing failed on the server: {ex.Message}" });
            }
        }

        // 4. DELETE: api/part-shipment/delete-line/12
        [HttpDelete("delete-line/{id}")]
        public async Task<IActionResult> DeleteLine([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Invalid target line identifier.");
            try
            {
                var success = await _partShipmentService.DeletePartShipmentLineAsync(id);
                return Ok(new { Success = success, Message = "Manifest line item purged and balances recalculated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to execute manifest line deletion loop: {ex.Message}" });
            }
        }
    }
}

