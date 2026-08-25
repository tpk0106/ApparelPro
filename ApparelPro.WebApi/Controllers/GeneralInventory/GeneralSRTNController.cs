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
    // endpoint for the item picker (same reuse pattern as GeneralGTNController/GeneralRTNController/GeneralDGNController).
    [Route("api/general-inventory-srtn")]
    [ApiController]
    [Authorize(Policy = "general-srtn")]
    public class GeneralSRTNController : ControllerBase
    {
        private readonly IGeneralSupplierReturnService _generalSupplierReturnService;
        private readonly IMapper _mapper;

        public GeneralSRTNController(IGeneralSupplierReturnService generalSupplierReturnService, IMapper mapper)
        {
            _generalSupplierReturnService = generalSupplierReturnService;
            _mapper = mapper;
        }

        // POST: api/general-inventory-srtn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitSupplierReturn([FromBody] GeneralSRTNAPIModel srtnAPIModel)
        {
            if (srtnAPIModel?.Header == null || srtnAPIModel.Lines == null || srtnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GeneralSrtnHeaderServiceModel>(srtnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GeneralSrtnLineItemServiceModel>>(srtnAPIModel.Lines);

            try
            {
                var success = await _generalSupplierReturnService.CommitGeneralSupplierReturnNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Supplier Return Note from Stores '{srtnAPIModel.Header.StoreCode}' to Supplier '{srtnAPIModel.Header.SupplierCode}' committed successfully."
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

        // GET: api/general-inventory-srtn/print?srtnNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string srtnNumber)
        {
            if (string.IsNullOrWhiteSpace(srtnNumber))
                return BadRequest("Parameter 'srtnNumber' is required.");

            try
            {
                var details = await _generalSupplierReturnService.GetGeneralSrtnPrintDetailsAsync(srtnNumber);
                return Ok(_mapper.Map<GeneralSrtnPrintDetailsAPIModel>(details));
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

        // GET: api/general-inventory-srtn/print/pdf?srtnNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string srtnNumber)
        {
            if (string.IsNullOrWhiteSpace(srtnNumber))
                return BadRequest("Parameter 'srtnNumber' is required.");

            try
            {
                var details = await _generalSupplierReturnService.GetGeneralSrtnPrintDetailsAsync(srtnNumber);
                byte[] pdfBytes = GeneralSrtnPrintEngine.GenerateSrtnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_SRTN_{srtnNumber}.pdf");
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
