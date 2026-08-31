using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.GeneralInventory
{
    // Reuses the existing GET api/general-inventory-strn/available-choices?storeCode=
    // endpoint for the item picker (same reuse pattern as GeneralGTNController/GeneralRTNController/GeneralDGNController/GeneralSRTNController).
    [Route("api/general-inventory-san")]
    [ApiController]
    [Authorize(Policy = "general-san")]
    public class GeneralSANController : ControllerBase
    {
        private readonly IGeneralStockAdjustmentService _generalStockAdjustmentService;
        private readonly IMapper _mapper;

        public GeneralSANController(IGeneralStockAdjustmentService generalStockAdjustmentService, IMapper mapper)
        {
            _generalStockAdjustmentService = generalStockAdjustmentService;
            _mapper = mapper;
        }

        // POST: api/general-inventory-san/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitStockAdjustment([FromBody] GeneralSANAPIModel sanAPIModel)
        {
            if (sanAPIModel?.Header == null || sanAPIModel.Lines == null || sanAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GeneralSanHeaderServiceModel>(sanAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GeneralSanLineItemServiceModel>>(sanAPIModel.Lines);

            try
            {
                var success = await _generalStockAdjustmentService.CommitGeneralStockAdjustmentNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Stock Adjustment Note for Stores '{sanAPIModel.Header.StoreCode}' committed successfully."
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

        // GET: api/general-inventory-san/print?sanNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string sanNumber)
        {
            if (string.IsNullOrWhiteSpace(sanNumber))
                return BadRequest("Parameter 'sanNumber' is required.");

            try
            {
                var details = await _generalStockAdjustmentService.GetGeneralSanPrintDetailsAsync(sanNumber);
                return Ok(_mapper.Map<GeneralSanPrintDetailsAPIModel>(details));
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

        // GET: api/general-inventory-san/print/pdf?sanNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string sanNumber)
        {
            if (string.IsNullOrWhiteSpace(sanNumber))
                return BadRequest("Parameter 'sanNumber' is required.");

            try
            {
                var details = await _generalStockAdjustmentService.GetGeneralSanPrintDetailsAsync(sanNumber);
                byte[] pdfBytes = GeneralSanPrintEngine.GenerateSanPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_SAN_{sanNumber}.pdf");
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
