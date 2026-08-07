using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public StyleApprovalController(IStyleApprovalService styleApprovalService, IMapper mapper)
        {
            _styleApprovalService = styleApprovalService;
            _mapper = mapper;
        }

        // GET: api/style-approval/details?buyerCode=&order=&typeCode=&styleCode=
        // NEW (2026-08-07) - backs the "Approve Trim Sheet" screen (Order Management ->
        // Material Consumption -> Approve Trim Sheet): lets the frontend show current
        // approval status before the user attempts to approve. Reads the exact same
        // Style.Username/ApprovedDate flag that Material Consumption and Supplier PO
        // raising already gate on server-side via GetStyleApprovalDetailsAsync - this
        // just exposes that same check as its own endpoint, nothing about the
        // underlying logic changes.
        [HttpGet("details")]
        [Authorize(Policy = "style-approval")]
        public async Task<IActionResult> GetDetails(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            var details = await _styleApprovalService.GetStyleApprovalDetailsAsync(buyerCode, order, typeCode, styleCode);
            if (details == null)
            {
                // Not yet approved - a plain 200/null response, not 404, since "unapproved"
                // is a normal, expected state here rather than an error.
                return Ok(null);
            }
            var apiModel = _mapper.Map<StyleApprovalDetailsAPIModel>(details);
            return Ok(apiModel);
        }

        // POST: api/style-approval/approve-events
        [HttpPost("approve-events")]
        [Authorize(Policy = "style-approval")]
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

        // POST: api/style-approval/approve-trim-sheet
        // NEW (2026-08-07) - the "Approve Trim Sheet" entry point (legacy OD_APRVL.PRG,
        // "TRIM SHEET APPROVAL", reached from "Material Consumptions -> Approval" in the
        // legacy menu). Calls the exact same service method as approve-events above -
        // they write the same Style.Username/ApprovedDate flag, there is genuinely only
        // one approval concept implemented server-side today (see StyleApprovalService's
        // comments on the Trim Sheet vs. Style-wise Events naming history). Kept as a
        // separate, correctly-named route rather than renaming approve-events in place,
        // so the existing Style-wise Events screen's call site doesn't need to change.
        [HttpPost("approve-trim-sheet")]
        [Authorize(Policy = "style-approval")]
        public async Task<IActionResult> ApproveTrimSheet([FromBody] StyleApprovalAPIModel styleApprovalAPIModel)
        {
            if (styleApprovalAPIModel == null)
                return BadRequest("The inbound approval model payload cannot be empty.");
            try
            {
                var success = await _styleApprovalService.ApproveStyleEventsAsync(
                    styleApprovalAPIModel.BuyerCode, styleApprovalAPIModel.Order, styleApprovalAPIModel.TypeCode, styleApprovalAPIModel.StyleCode,
                    styleApprovalAPIModel.ApprovedByUserId, styleApprovalAPIModel.ApprovalDate
                );
                return Ok(new { Success = success, Message = "Trim Sheet approved successfully for the selected style." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transaction processing error encountered: {ex.Message}" });
            }
        }
    }
}
