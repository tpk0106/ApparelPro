using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.GeneralInventory
{
    [Route("api/general-inventory-grn")]
    [ApiController]
    [Authorize(Policy = "general-grn")]
    public class GeneralGRNController : ControllerBase
    {
        private readonly IGeneralGoodsReceivedService _generalGoodsReceivedService;
        private readonly IMapper _mapper;

        public GeneralGRNController(IGeneralGoodsReceivedService generalGoodsReceivedService, IMapper mapper)
        {
            _generalGoodsReceivedService = generalGoodsReceivedService;
            _mapper = mapper;
        }

        // GET: api/general-inventory-grn/receivable-lines?poNumber=000001
        [HttpGet("receivable-lines")]
        public async Task<IActionResult> GetReceivableLines([FromQuery] string poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return BadRequest("Parameter 'poNumber' is required.");

            try
            {
                var result = await _generalGoodsReceivedService.GetReceivableLinesByPoAsync(poNumber);
                return Ok(_mapper.Map<GeneralGrnPoLookupResultAPIModel>(result));
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
                return StatusCode(500, new { Error = $"Failed to load receivable P/O lines: {ex.Message}" });
            }
        }

        // POST: api/general-inventory-grn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitGoodsReceived([FromBody] GeneralGRNAPIModel grnAPIModel)
        {
            if (grnAPIModel?.Header == null || grnAPIModel.Lines == null || grnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GeneralGrnHeaderServiceModel>(grnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GeneralGrnLineItemServiceModel>>(grnAPIModel.Lines);

            try
            {
                var success = await _generalGoodsReceivedService.CommitGeneralGoodsReceivedNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername,
                    grnAPIModel.MaxStockOverrideConfirmed);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Received Note against P/O '{grnAPIModel.Header.PoNumber}' committed successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (MaxStockOverrideRequiredException ex)
            {
                // Distinct status so the frontend can show a plain confirm dialog instead
                // of a flat error toast - no role check needed, matching legacy's own
                // ungated Yes/No prompt.
                return StatusCode(409, new { RequiresOverride = true, Error = ex.Message });
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

        // GET: api/general-inventory-grn/print?grnNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string grnNumber)
        {
            if (string.IsNullOrWhiteSpace(grnNumber))
                return BadRequest("Parameter 'grnNumber' is required.");

            try
            {
                var details = await _generalGoodsReceivedService.GetGeneralGrnPrintDetailsAsync(grnNumber);
                return Ok(_mapper.Map<GeneralGrnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Goods Received Note: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-grn/print/pdf?grnNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string grnNumber)
        {
            if (string.IsNullOrWhiteSpace(grnNumber))
                return BadRequest("Parameter 'grnNumber' is required.");

            try
            {
                var details = await _generalGoodsReceivedService.GetGeneralGrnPrintDetailsAsync(grnNumber);
                byte[] pdfBytes = GeneralGrnPrintEngine.GenerateGrnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_GRN_{grnNumber}.pdf");
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
