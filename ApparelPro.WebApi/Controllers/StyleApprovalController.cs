using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.OrderManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/style-approval")]
    [ApiController]
    public class StyleApprovalController : ControllerBase
    {
        private readonly IStyleApprovalService _styleApprovalService;

        public StyleApprovalController(IStyleApprovalService styleApprovalService)
        {
            _styleApprovalService = styleApprovalService;
        }

        // POST: api/style-approval/approve-events
        [HttpPost("approve-events")]
        [Authorize(Roles = "Merchandiser Manager")]
        public async Task<IActionResult> ApproveEvents([FromBody] StyleApprovalAPIModel styleApprovalAPIModel)
        {
            if (styleApprovalAPIModel == null) 
                return BadRequest("The inbound authorization model payload cannot be empty.");
            try
            {
                var success = await _styleApprovalService.ApproveStyleEventsAsync(
                    styleApprovalAPIModel.BuyerCode, styleApprovalAPIModel.Order, styleApprovalAPIModel.TypeCode, styleApprovalAPIModel.StyleCode,
                    styleApprovalAPIModel.ApprovedByUserId, styleApprovalAPIModel.ApprovalDate
                );
                return Ok(new { Success = success, Message = "Milestone timeline successfully signed-off and locked by executive authority." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message }); // Returns the clear descriptive blocker messages safely
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transaction processing error encountered: {ex.Message}" });
            }
        }
    }
}
