using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using ApparelPro.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/orderwise-inventory-grn")]
    [ApiController]
    [Authorize(Roles = AccessPolicies.OrderwiseInventoryStandard)]
    public class GRNController : ControllerBase
    {
        private readonly IGoodsReceivedNoteService _goodsReceivedNoteService;
        private readonly IMapper _mapper;

        public GRNController(IGoodsReceivedNoteService goodsReceivedNoteService, IMapper mapper)
        {
            _goodsReceivedNoteService = goodsReceivedNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-grn/receivable-lines?poNumber=000001
        [HttpGet("receivable-lines")]
        public async Task<IActionResult> GetReceivableLines([FromQuery] string poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return BadRequest("Parameter 'poNumber' is required.");

            try
            {
                var result = await _goodsReceivedNoteService.GetReceivableLinesByPoAsync(poNumber);
                var resultAPIModel = _mapper.Map<GrnPoLookupResultAPIModel>(result);
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
                return StatusCode(500, new { Error = $"Failed to load receivable PO lines: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-grn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitGoodsReceipt([FromBody] GrnAPIModel grnAPIModel)
        {
            if (grnAPIModel == null)
                return BadRequest("The inbound Goods Received Note payload cannot be empty.");

            if (grnAPIModel.Header == null || grnAPIModel.Lines == null || grnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GrnHeaderServiceModel>(grnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GrnLineItemServiceModel>>(grnAPIModel.Lines);

            try
            {
                var success = await _goodsReceivedNoteService.CommitGoodsReceivedNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Received Note against P/O '{grnAPIModel.Header.PurchaseOrderNumber}' committed and inventory balances updated successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
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

        // 3. GET: api/orderwise-inventory-grn/pending-pos?buyerCode=1&order=1017-18
        // Additive alternate entry point for a dropdown-browse UI — does not replace or
        // modify receivable-lines/commit.
        [HttpGet("pending-pos")]
        public async Task<IActionResult> GetPendingPos([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var pendingPos = await _goodsReceivedNoteService.GetPendingPosByOrderAsync(buyerCode, order);
                var pendingPosAPIModel = _mapper.Map<List<GrnPendingPoAPIModel>>(pendingPos);
                return Ok(pendingPosAPIModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load pending POs: {ex.Message}" });
            }
        }
    }
}
