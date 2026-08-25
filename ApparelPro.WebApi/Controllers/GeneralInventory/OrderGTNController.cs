using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.GeneralInventory
{
    // Goods Transfer Note (Orders) - the General Inventory <-> Orderwise Inventory bridge.
    [Route("api/general-inventory-ogtn")]
    [ApiController]
    [Authorize(Policy = "general-ogtn")]
    public class OrderGTNController : ControllerBase
    {
        private readonly IOrderGoodsTransferService _orderGoodsTransferService;
        private readonly IMapper _mapper;

        public OrderGTNController(IOrderGoodsTransferService orderGoodsTransferService, IMapper mapper)
        {
            _orderGoodsTransferService = orderGoodsTransferService;
            _mapper = mapper;
        }

        // GET: api/general-inventory-ogtn/transferable-stock?direction=GeneralToOrder&storeCode=M-S&buyerCode=1&order=ORD001
        [HttpGet("transferable-stock")]
        public async Task<IActionResult> GetTransferableStock(
            [FromQuery] string direction,
            [FromQuery] string storeCode,
            [FromQuery] int buyerCode,
            [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(direction) || string.IsNullOrWhiteSpace(storeCode) || string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameters 'direction', 'storeCode' and 'order' are required.");

            try
            {
                var result = await _orderGoodsTransferService.GetTransferableStockAsync(direction, storeCode, buyerCode, order);
                return Ok(_mapper.Map<List<OrderGtnTransferableStockRowAPIModel>>(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load transferable stock: {ex.Message}" });
            }
        }

        // POST: api/general-inventory-ogtn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitOrderGoodsTransfer([FromBody] OrderGTNAPIModel ogtnAPIModel)
        {
            if (ogtnAPIModel?.Header == null || ogtnAPIModel.Lines == null || ogtnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<OrderGtnHeaderServiceModel>(ogtnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<OrderGtnLineItemServiceModel>>(ogtnAPIModel.Lines);

            try
            {
                var success = await _orderGoodsTransferService.CommitOrderGoodsTransferNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Transfer Note (Orders) for Buyer/Order '{ogtnAPIModel.Header.BuyerCode}/{ogtnAPIModel.Header.Order}' committed successfully."
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
                return StatusCode(500, new { Error = $"Transaction processing failed: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-ogtn/print?ogtnNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string ogtnNumber)
        {
            if (string.IsNullOrWhiteSpace(ogtnNumber))
                return BadRequest("Parameter 'ogtnNumber' is required.");

            try
            {
                var details = await _orderGoodsTransferService.GetOrderGtnPrintDetailsAsync(ogtnNumber);
                return Ok(_mapper.Map<OrderGtnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Goods Transfer Note (Orders): {ex.Message}" });
            }
        }

        // GET: api/general-inventory-ogtn/print/pdf?ogtnNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string ogtnNumber)
        {
            if (string.IsNullOrWhiteSpace(ogtnNumber))
                return BadRequest("Parameter 'ogtnNumber' is required.");

            try
            {
                var details = await _orderGoodsTransferService.GetOrderGtnPrintDetailsAsync(ogtnNumber);
                byte[] pdfBytes = OrderGtnPrintEngine.GenerateOgtnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_OGTN_{ogtnNumber}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to generate PDF: {ex.Message}" });
            }
        }
    }
}
