using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using ApparelPro.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/orderwise-inventory-gin")]
    [ApiController]
    [Authorize(Roles = AccessPolicies.OrderwiseInventoryStandard)]
    public class GINController : ControllerBase
    {
        private readonly IGoodsIssueService _goodsIssueService;
        private readonly IMapper _mapper;

        public GINController(IGoodsIssueService goodsIssueService, IMapper mapper)
        {
            _goodsIssueService = goodsIssueService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-gin/issuable-lines?strnNumber=STRN000123
        [HttpGet("issuable-lines")]
        public async Task<IActionResult> GetIssuableLines([FromQuery] string strnNumber)
        {
            if (string.IsNullOrWhiteSpace(strnNumber))
                return BadRequest("Parameter 'strnNumber' is required.");

            try
            {
                var result = await _goodsIssueService.GetIssuableStrnLinesAsync(strnNumber);
                var resultAPIModel = _mapper.Map<GinStrnLookupResultAPIModel>(result);
                return Ok(resultAPIModel);
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
                return StatusCode(500, new { Error = $"Failed to load issuable STRN lines: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-gin/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitGoodsIssue([FromBody] GinAPIModel ginAPIModel)
        {
            if (ginAPIModel == null)
                return BadRequest("The inbound Goods Issue Note payload cannot be empty.");

            if (ginAPIModel.Header == null || ginAPIModel.Lines == null || ginAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            // Server-side authority check — never trust a client-supplied override flag on
            // its own. Only a user actually holding the "Merchandiser Manager" role can
            // authorize bypassing the exact-consumption soft check.
            bool isManagerOverrideAuthorized = User.IsInRole("Merchandiser Manager");

            var headerServiceModel = _mapper.Map<GinHeaderServiceModel>(ginAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GinLineItemServiceModel>>(ginAPIModel.Lines);

            try
            {
                var success = await _goodsIssueService.CommitGoodsIssueNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername,
                    isManagerOverrideAuthorized,
                    ginAPIModel.OverrideExactConsumptionCheck);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Issue Note against STRN '{ginAPIModel.Header.SourceStrnNumber}' committed and inventory balances updated successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (ExactConsumptionOverrideRequiredException ex)
            {
                // Distinct status so the frontend can show an override-confirm dialog
                // instead of a flat error toast.
                return StatusCode(409, new { RequiresOverride = true, Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transaction processing failed on the SQL server: {ex.Message}" });
            }
        }

        // 3. GET: api/orderwise-inventory-gin/pending-strns?buyerCode=2&order=1017-18
        // Additive alternate entry point for a dropdown-browse UI — does not replace or
        // modify issuable-lines/commit.
        [HttpGet("pending-strns")]
        public async Task<IActionResult> GetPendingStrns([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var pendingStrns = await _goodsIssueService.GetPendingStrnsByOrderAsync(buyerCode, order);
                var pendingStrnsAPIModel = _mapper.Map<List<GinPendingStrnAPIModel>>(pendingStrns);
                return Ok(pendingStrnsAPIModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load pending STRNs: {ex.Message}" });
            }
        }
    }
}
