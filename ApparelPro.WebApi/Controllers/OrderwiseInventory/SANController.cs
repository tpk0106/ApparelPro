using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using ApparelPro.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.OrderwiseInventory
{
    [Route("api/orderwise-inventory-san")]
    [ApiController]
    [Authorize(Policy = "san")]
    public class SANController : ControllerBase
    {
        private readonly IStockAdjustmentNoteService _stockAdjustmentNoteService;
        private readonly IMapper _mapper;

        public SANController(IStockAdjustmentNoteService stockAdjustmentNoteService, IMapper mapper)
        {
            _stockAdjustmentNoteService = stockAdjustmentNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-san/adjustable-stock?buyerCode=1&order=1017-18
        // Powers the item picker — one row per (Store, Item) for this Buyer/Order,
        // regardless of current QtyInHand (a stock take can correct any item's count).
        [HttpGet("adjustable-stock")]
        public async Task<IActionResult> GetAdjustableStock(
            [FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var result = await _stockAdjustmentNoteService.GetAdjustableStockByBuyerOrderAsync(buyerCode, order);
                var resultAPIModel = _mapper.Map<List<SanAdjustableStockRowAPIModel>>(result);
                return Ok(resultAPIModel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load adjustable stock: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-san/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitStockAdjustment([FromBody] SanAPIModel sanAPIModel)
        {
            if (sanAPIModel == null)
                return BadRequest("The inbound Stock Adjustment Note payload cannot be empty.");

            if (sanAPIModel.Header == null || sanAPIModel.Lines == null || sanAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<SanHeaderServiceModel>(sanAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<SanLineItemServiceModel>>(sanAPIModel.Lines);

            try
            {
                var success = await _stockAdjustmentNoteService.CommitStockAdjustmentNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Stock Adjustment Note for Order '{sanAPIModel.Header.Order}' committed and inventory balances updated successfully.",
                    SanNumber = headerServiceModel.SanNumber
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

        // GET: api/orderwise-inventory-san/print?sanNumber=000123
        [HttpGet("print")]
        public async Task<IActionResult> GetSanPrintDetails([FromQuery] string sanNumber)
        {
            if (string.IsNullOrWhiteSpace(sanNumber))
                return BadRequest("Parameter 'sanNumber' is required.");

            try
            {
                var details = await _stockAdjustmentNoteService.GetSanPrintDetailsAsync(sanNumber);
                return Ok(_mapper.Map<SanPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Stock Adjustment Note: {ex.Message}" });
            }
        }

        // GET: api/orderwise-inventory-san/print/pdf?sanNumber=000123
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetSanPrintPdf([FromQuery] string sanNumber)
        {
            if (string.IsNullOrWhiteSpace(sanNumber))
                return BadRequest("Parameter 'sanNumber' is required.");

            try
            {
                var details = await _stockAdjustmentNoteService.GetSanPrintDetailsAsync(sanNumber);
                byte[] pdfBytes = SanPrintEngine.GenerateSanPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"SAN_{sanNumber}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to generate Stock Adjustment Note PDF: {ex.Message}" });
            }
        }
    }
}
