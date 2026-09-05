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
    [Route("api/orderwise-inventory-rtn")]
    [ApiController]
    [Authorize(Policy = "rtn")]
    public class RTNController : ControllerBase
    {
        private readonly IGoodsReturnNoteService _goodsReturnNoteService;
        private readonly IMapper _mapper;

        public RTNController(IGoodsReturnNoteService goodsReturnNoteService, IMapper mapper)
        {
            _goodsReturnNoteService = goodsReturnNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-rtn/returnable-stock?buyerCode=1&order=1017-18
        // Powers the item picker — one row per (Store, Item) with something left to return.
        [HttpGet("returnable-stock")]
        public async Task<IActionResult> GetReturnableStock([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var result = await _goodsReturnNoteService.GetReturnableStockByBuyerOrderAsync(buyerCode, order);
                var resultAPIModel = _mapper.Map<List<RtnReturnableStockRowAPIModel>>(result);
                return Ok(resultAPIModel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load returnable stock: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-rtn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitGoodsReturn([FromBody] RtnAPIModel rtnAPIModel)
        {
            if (rtnAPIModel == null)
                return BadRequest("The inbound Goods Return Note payload cannot be empty.");

            if (rtnAPIModel.Header == null || rtnAPIModel.Lines == null || rtnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<RtnHeaderServiceModel>(rtnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<RtnLineItemServiceModel>>(rtnAPIModel.Lines);

            try
            {
                var success = await _goodsReturnNoteService.CommitGoodsReturnNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Return Note against Order '{rtnAPIModel.Header.Order}' committed and inventory balances updated successfully.",
                    RtnNumber = headerServiceModel.RtnNumber
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

        // GET: api/orderwise-inventory-rtn/print?rtnNumber=000123
        [HttpGet("print")]
        public async Task<IActionResult> GetRtnPrintDetails([FromQuery] string rtnNumber)
        {
            if (string.IsNullOrWhiteSpace(rtnNumber))
                return BadRequest("Parameter 'rtnNumber' is required.");

            try
            {
                var details = await _goodsReturnNoteService.GetRtnPrintDetailsAsync(rtnNumber);
                return Ok(_mapper.Map<RtnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Goods Return Note: {ex.Message}" });
            }
        }

        // GET: api/orderwise-inventory-rtn/print/pdf?rtnNumber=000123
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetRtnPrintPdf([FromQuery] string rtnNumber)
        {
            if (string.IsNullOrWhiteSpace(rtnNumber))
                return BadRequest("Parameter 'rtnNumber' is required.");

            try
            {
                var details = await _goodsReturnNoteService.GetRtnPrintDetailsAsync(rtnNumber);
                byte[] pdfBytes = RtnPrintEngine.GenerateRtnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"RTN_{rtnNumber}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to generate Goods Return Note PDF: {ex.Message}" });
            }
        }
    }
}
