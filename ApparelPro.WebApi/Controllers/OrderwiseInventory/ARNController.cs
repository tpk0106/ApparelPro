using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.OrderwiseInventory
{
    [Route("api/orderwise-inventory-arn")]
    [ApiController]
    [Authorize(Policy = "arn")]
    public class ARNController : ControllerBase
    {
        private readonly IAdditionalGoodsReceiptNoteService _additionalGoodsReceiptNoteService;
        private readonly IMapper _mapper;

        public ARNController(IAdditionalGoodsReceiptNoteService additionalGoodsReceiptNoteService, IMapper mapper)
        {
            _additionalGoodsReceiptNoteService = additionalGoodsReceiptNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-arn/receivable-stock?buyerCode=2&order=1017-18
        // Powers the item picker - one row per (Store, Item, Process) for this Buyer/Order
        // with an Additional Process assignment.
        [HttpGet("receivable-stock")]
        public async Task<IActionResult> GetReceivableStock(
            [FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var result = await _additionalGoodsReceiptNoteService.GetReceivableStockByBuyerOrderAsync(buyerCode, order);
                var resultAPIModel = _mapper.Map<List<ArnReceivableStockRowAPIModel>>(result);
                return Ok(resultAPIModel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load receivable stock: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-arn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitAdditionalGoodsReceipt([FromBody] ArnAPIModel arnAPIModel)
        {
            if (arnAPIModel == null)
                return BadRequest("The inbound Additional Goods Receipt Note payload cannot be empty.");

            if (arnAPIModel.Header == null || arnAPIModel.Lines == null || arnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<ArnHeaderServiceModel>(arnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<ArnLineItemServiceModel>>(arnAPIModel.Lines);

            try
            {
                var success = await _additionalGoodsReceiptNoteService.CommitAdditionalGoodsReceiptNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = "Additional Goods Receipt Note committed and inventory balances updated successfully.",
                    ArnNumber = headerServiceModel.ArnNumber
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

        // 3. GET: api/orderwise-inventory-arn/print?arnNumber=000123
        [HttpGet("print")]
        public async Task<IActionResult> GetArnPrintDetails([FromQuery] string arnNumber)
        {
            if (string.IsNullOrWhiteSpace(arnNumber))
                return BadRequest("Parameter 'arnNumber' is required.");

            try
            {
                var details = await _additionalGoodsReceiptNoteService.GetArnPrintDetailsAsync(arnNumber);
                return Ok(_mapper.Map<ArnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Additional Goods Receipt Note: {ex.Message}" });
            }
        }

        // 4. GET: api/orderwise-inventory-arn/print/pdf?arnNumber=000123
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetArnPrintPdf([FromQuery] string arnNumber)
        {
            if (string.IsNullOrWhiteSpace(arnNumber))
                return BadRequest("Parameter 'arnNumber' is required.");

            try
            {
                var details = await _additionalGoodsReceiptNoteService.GetArnPrintDetailsAsync(arnNumber);
                byte[] pdfBytes = ArnPrintEngine.GenerateArnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"ARN_{arnNumber}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to generate Additional Goods Receipt Note PDF: {ex.Message}" });
            }
        }
    }
}
