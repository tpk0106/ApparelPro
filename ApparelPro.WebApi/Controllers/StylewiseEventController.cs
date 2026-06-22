using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.OrderManagement;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/stylewise-event")]
    [ApiController]
    public class StylewiseEventController : ControllerBase
    {
        private readonly IStylewiseEventService _stylewiseEventService;
        private readonly IMapper _mapper;

        public StylewiseEventController(IStylewiseEventService stylewiseEventService, IMapper mapper)
        {
            _stylewiseEventService = stylewiseEventService;
            _mapper = mapper;
        }

        // 1. GET: api/stylewise-event/checklist?buyerCode=2&order=1017-18&typeCode=2&styleCode=M102
        [HttpGet("checklist")]
        public async Task<IActionResult> GetChecklist(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(styleCode))
                return BadRequest("Tracking parameters 'order' and 'styleCode' cannot be empty.");

            try
            {
                var checklist = await _stylewiseEventService.GetStyleEventsAsync(buyerCode, order, typeCode, styleCode);
                return Ok(checklist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to build stylewise event checklist matrix: {ex.Message}" });
            }
        }

        // 2. PUT: api/stylewise-event/update-line
        [HttpPut("update-line")]
        public async Task<IActionResult> UpdateLine([FromBody] UpdateEventLineAPIModel request)
        {
            if (request == null) return BadRequest("Payload cannot be empty.");
            try
            {
                var success = await _stylewiseEventService.UpdateStyleEventLineAsync(
                    request.BuyerCode, request.Order, request.TypeCode, request.StyleCode,
                    request.EventCode, request.ScheduledDate, request.ActualDate, request.Remarks
                );

                if (!success) return NotFound("Target event milestone row could not be found to update.");
                return Ok(new { Success = true, Message = "Milestone line modified successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to execute inline milestone update: {ex.Message}" });
            }
        }

        // 3. POST: api/stylewise-event/add-custom
        [HttpPost("add-custom")]
        public async Task<IActionResult> AddCustomLine([FromBody] AddCustomEventLineAPIModel request)
        {
            if (request == null) return BadRequest("Payload cannot be empty.");
            try
            {
                var success = await _stylewiseEventService.AddCustomStyleEventLineAsync(
                    request.BuyerCode, request.Order, request.TypeCode, request.StyleCode,
                    request.EventCode, request.ScheduledDate, request.ActualDate, request.Remarks
                );

                if (!success) return BadRequest("A milestone row with that unique Event Code is already present under this style.");
                return Ok(new { Success = true, Message = "Custom tracking milestone appended cleanly." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message }); // Returns management lockdown messages
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to insert custom event milestone: {ex.Message}" });
            }
        }

        // 4. DELETE: api/stylewise-event/delete-line
        [HttpDelete("delete-line")]
        public async Task<IActionResult> DeleteLine(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode,
            [FromQuery] string eventCode)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(eventCode))
                return BadRequest("Missing required query tracking identifier keys.");

            try
            {
                await _stylewiseEventService.DeleteStyleEventLineAsync(buyerCode, order, typeCode, styleCode, eventCode);
                return Ok(new { Success = true, Message = "Milestone line purged from EventMasters successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message }); // Handles approval protection drops
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to delete event milestone line item: {ex.Message}" });
            }
        }
    }
}
