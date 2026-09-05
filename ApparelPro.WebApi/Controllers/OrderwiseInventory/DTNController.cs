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
    [Route("api/orderwise-inventory-dtn")]
    [ApiController]
    [Authorize(Policy = "dtn")]
    public class DTNController : ControllerBase
    {
        private readonly IDirectTransferNoteService _directTransferNoteService;
        private readonly IMapper _mapper;

        public DTNController(IDirectTransferNoteService directTransferNoteService, IMapper mapper)
        {
            _directTransferNoteService = directTransferNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-dtn/from-stock?fromBuyerCode=1&fromOrder=1017-18&toBuyerCode=2&toOrder=1018-19
        // Powers the source-item picker — one row per (Store, Item) with something to transfer.
        [HttpGet("from-stock")]
        public async Task<IActionResult> GetFromStock(
            [FromQuery] int fromBuyerCode, [FromQuery] string fromOrder,
            [FromQuery] int toBuyerCode, [FromQuery] string toOrder)
        {
            if (string.IsNullOrWhiteSpace(fromOrder) || string.IsNullOrWhiteSpace(toOrder))
                return BadRequest("Parameters 'fromOrder' and 'toOrder' are required.");

            try
            {
                var result = await _directTransferNoteService.GetFromStockAsync(fromBuyerCode, fromOrder, toBuyerCode, toOrder);
                var resultAPIModel = _mapper.Map<List<DtnFromStockRowAPIModel>>(result);
                return Ok(resultAPIModel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load From-side stock: {ex.Message}" });
            }
        }

        // 2. GET: api/orderwise-inventory-dtn/to-order-items?toBuyerCode=2&toOrder=1018-19
        // Powers the "map to this item" dropdown per line, sourced from the To Order's own
        // material requirement.
        [HttpGet("to-order-items")]
        public async Task<IActionResult> GetToOrderItems([FromQuery] int toBuyerCode, [FromQuery] string toOrder)
        {
            if (string.IsNullOrWhiteSpace(toOrder))
                return BadRequest("Parameter 'toOrder' is required.");

            try
            {
                var result = await _directTransferNoteService.GetToOrderItemsAsync(toBuyerCode, toOrder);
                var resultAPIModel = _mapper.Map<List<DtnToItemAPIModel>>(result);
                return Ok(resultAPIModel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load To Order items: {ex.Message}" });
            }
        }

        // 3. POST: api/orderwise-inventory-dtn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitDirectTransfer([FromBody] DtnAPIModel dtnAPIModel)
        {
            if (dtnAPIModel == null)
                return BadRequest("The inbound Direct Goods Transfer Note payload cannot be empty.");

            if (dtnAPIModel.Header == null || dtnAPIModel.Lines == null || dtnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<DtnHeaderServiceModel>(dtnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<DtnLineItemServiceModel>>(dtnAPIModel.Lines);

            try
            {
                var success = await _directTransferNoteService.CommitDirectTransferNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Direct Goods Transfer Note from Order '{dtnAPIModel.Header.FromOrder}' to Order '{dtnAPIModel.Header.ToOrder}' committed and inventory balances updated successfully.",
                    DtnNumber = headerServiceModel.DtnNumber
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

        // GET: api/orderwise-inventory-dtn/print?dtnNumber=000123
        [HttpGet("print")]
        public async Task<IActionResult> GetDtnPrintDetails([FromQuery] string dtnNumber)
        {
            if (string.IsNullOrWhiteSpace(dtnNumber))
                return BadRequest("Parameter 'dtnNumber' is required.");

            try
            {
                var details = await _directTransferNoteService.GetDtnPrintDetailsAsync(dtnNumber);
                return Ok(_mapper.Map<DtnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Direct Goods Transfer Note: {ex.Message}" });
            }
        }

        // GET: api/orderwise-inventory-dtn/print/pdf?dtnNumber=000123
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetDtnPrintPdf([FromQuery] string dtnNumber)
        {
            if (string.IsNullOrWhiteSpace(dtnNumber))
                return BadRequest("Parameter 'dtnNumber' is required.");

            try
            {
                var details = await _directTransferNoteService.GetDtnPrintDetailsAsync(dtnNumber);
                byte[] pdfBytes = DtnPrintEngine.GenerateDtnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"DTN_{dtnNumber}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to generate Direct Goods Transfer Note PDF: {ex.Message}" });
            }
        }
    }
}
