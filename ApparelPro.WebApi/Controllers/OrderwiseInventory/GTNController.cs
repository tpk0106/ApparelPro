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
    [Route("api/orderwise-inventory-gtn")]
    [ApiController]
    [Authorize(Policy = "gtn")]
    public class GTNController : ControllerBase
    {
        private readonly IGoodsTransferNoteService _goodsTransferNoteService;
        private readonly IMapper _mapper;

        public GTNController(IGoodsTransferNoteService goodsTransferNoteService, IMapper mapper)
        {
            _goodsTransferNoteService = goodsTransferNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-gtn/transferable-stock?fromBuyerCode=1&fromOrder=1017-18&toBuyerCode=1&toOrder=1018-19
        // Powers the item picker — one row per (Store, Item) that can be transferred From -> To.
        [HttpGet("transferable-stock")]
        public async Task<IActionResult> GetTransferableStock(
            [FromQuery] int fromBuyerCode, [FromQuery] string fromOrder,
            [FromQuery] int toBuyerCode, [FromQuery] string toOrder)
        {
            if (string.IsNullOrWhiteSpace(fromOrder) || string.IsNullOrWhiteSpace(toOrder))
                return BadRequest("Parameters 'fromOrder' and 'toOrder' are required.");

            try
            {
                var result = await _goodsTransferNoteService.GetTransferableStockAsync(fromBuyerCode, fromOrder, toBuyerCode, toOrder);
                var resultAPIModel = _mapper.Map<List<GtnTransferableStockRowAPIModel>>(result);
                return Ok(resultAPIModel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load transferable stock: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-gtn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitGoodsTransfer([FromBody] GtnAPIModel gtnAPIModel)
        {
            if (gtnAPIModel == null)
                return BadRequest("The inbound Goods Transfer Note payload cannot be empty.");

            if (gtnAPIModel.Header == null || gtnAPIModel.Lines == null || gtnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GtnHeaderServiceModel>(gtnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GtnLineItemServiceModel>>(gtnAPIModel.Lines);

            try
            {
                var success = await _goodsTransferNoteService.CommitGoodsTransferNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Transfer Note from Order '{gtnAPIModel.Header.FromOrder}' to Order '{gtnAPIModel.Header.ToOrder}' committed and inventory balances updated successfully.",
                    GtnNumber = headerServiceModel.GtnNumber
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

        // GET: api/orderwise-inventory-gtn/print?gtnNumber=000123
        [HttpGet("print")]
        public async Task<IActionResult> GetGtnPrintDetails([FromQuery] string gtnNumber)
        {
            if (string.IsNullOrWhiteSpace(gtnNumber))
                return BadRequest("Parameter 'gtnNumber' is required.");

            try
            {
                var details = await _goodsTransferNoteService.GetGtnPrintDetailsAsync(gtnNumber);
                return Ok(_mapper.Map<GtnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Goods Transfer Note: {ex.Message}" });
            }
        }

        // GET: api/orderwise-inventory-gtn/print/pdf?gtnNumber=000123
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetGtnPrintPdf([FromQuery] string gtnNumber)
        {
            if (string.IsNullOrWhiteSpace(gtnNumber))
                return BadRequest("Parameter 'gtnNumber' is required.");

            try
            {
                var details = await _goodsTransferNoteService.GetGtnPrintDetailsAsync(gtnNumber);
                byte[] pdfBytes = GtnPrintEngine.GenerateGtnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"GTN_{gtnNumber}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to generate Goods Transfer Note PDF: {ex.Message}" });
            }
        }
    }
}
