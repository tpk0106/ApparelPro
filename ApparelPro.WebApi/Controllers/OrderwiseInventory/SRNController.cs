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
    [Route("api/orderwise-inventory-srn")]
    [ApiController]
    [Authorize(Policy = "srn")]
    public class SRNController : ControllerBase
    {
        private readonly ISupplierReturnNoteService _supplierReturnNoteService;
        private readonly IMapper _mapper;

        public SRNController(ISupplierReturnNoteService supplierReturnNoteService, IMapper mapper)
        {
            _supplierReturnNoteService = supplierReturnNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-srn/returnable-stock?buyerCode=1&order=1017-18
        // Powers the item picker — one row per (Store, Item) that currently has stock on hand
        // and can be returned to a supplier.
        [HttpGet("returnable-stock")]
        public async Task<IActionResult> GetReturnableStock(
            [FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var result = await _supplierReturnNoteService.GetReturnableStockByBuyerOrderAsync(buyerCode, order);
                var resultAPIModel = _mapper.Map<List<SrnReturnableStockRowAPIModel>>(result);
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

        // 2. POST: api/orderwise-inventory-srn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitSupplierReturn([FromBody] SrnAPIModel srnAPIModel)
        {
            if (srnAPIModel == null)
                return BadRequest("The inbound Supplier Return Note payload cannot be empty.");

            if (srnAPIModel.Header == null || srnAPIModel.Lines == null || srnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<SrnHeaderServiceModel>(srnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<SrnLineItemServiceModel>>(srnAPIModel.Lines);

            try
            {
                var success = await _supplierReturnNoteService.CommitSupplierReturnNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Supplier Return Note for Order '{srnAPIModel.Header.Order}' committed and inventory balances updated successfully.",
                    SrnNumber = headerServiceModel.SrnNumber
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

        // GET: api/orderwise-inventory-srn/print?srnNumber=000123
        [HttpGet("print")]
        public async Task<IActionResult> GetSrnPrintDetails([FromQuery] string srnNumber)
        {
            if (string.IsNullOrWhiteSpace(srnNumber))
                return BadRequest("Parameter 'srnNumber' is required.");

            try
            {
                var details = await _supplierReturnNoteService.GetSrnPrintDetailsAsync(srnNumber);
                return Ok(_mapper.Map<SrnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Supplier Return Note: {ex.Message}" });
            }
        }

        // GET: api/orderwise-inventory-srn/print/pdf?srnNumber=000123
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetSrnPrintPdf([FromQuery] string srnNumber)
        {
            if (string.IsNullOrWhiteSpace(srnNumber))
                return BadRequest("Parameter 'srnNumber' is required.");

            try
            {
                var details = await _supplierReturnNoteService.GetSrnPrintDetailsAsync(srnNumber);
                byte[] pdfBytes = SrnPrintEngine.GenerateSrnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"SRN_{srnNumber}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to generate Supplier Return Note PDF: {ex.Message}" });
            }
        }
    }
}
