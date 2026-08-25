using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.GeneralInventory
{
    [Route("api/general-inventory-gin")]
    [ApiController]
    [Authorize(Policy = "general-gin")]
    public class GeneralGINController : ControllerBase
    {
        private readonly IGeneralGoodsIssueService _generalGoodsIssueService;
        private readonly IMapper _mapper;

        public GeneralGINController(IGeneralGoodsIssueService generalGoodsIssueService, IMapper mapper)
        {
            _generalGoodsIssueService = generalGoodsIssueService;
            _mapper = mapper;
        }

        // GET: api/general-inventory-gin/issuable-lines?strnNumber=000001
        [HttpGet("issuable-lines")]
        public async Task<IActionResult> GetIssuableLines([FromQuery] string strnNumber)
        {
            if (string.IsNullOrWhiteSpace(strnNumber))
                return BadRequest("Parameter 'strnNumber' is required.");

            try
            {
                var result = await _generalGoodsIssueService.GetIssuableStrnLinesAsync(strnNumber);
                return Ok(_mapper.Map<GeneralGinStrnLookupResultAPIModel>(result));
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
                return StatusCode(500, new { Error = $"Failed to load issuable SRN lines: {ex.Message}" });
            }
        }

        // POST: api/general-inventory-gin/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitGoodsIssue([FromBody] GeneralGinAPIModel ginAPIModel)
        {
            if (ginAPIModel?.Header == null || ginAPIModel.Lines == null || ginAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            // Server-side authority check - never trust a client-supplied override flag on
            // its own. Only a user actually holding the "Merchandiser Manager" role can
            // authorize bypassing the minimum-stock soft check.
            bool isManagerOverrideAuthorized = User.IsInRole("Merchandiser Manager");

            var headerServiceModel = _mapper.Map<GeneralGinHeaderServiceModel>(ginAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GeneralGinLineItemServiceModel>>(ginAPIModel.Lines);

            try
            {
                var success = await _generalGoodsIssueService.CommitGeneralGoodsIssueNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername,
                    isManagerOverrideAuthorized,
                    ginAPIModel.OverrideMinStockCheck);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Issue Note against SRN '{ginAPIModel.Header.SourceStrnNumber}' committed successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (MinStockOverrideRequiredException ex)
            {
                // Distinct status so the frontend can show an override-confirm dialog
                // instead of a flat error toast.
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

        // GET: api/general-inventory-gin/print?ginNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string ginNumber)
        {
            if (string.IsNullOrWhiteSpace(ginNumber))
                return BadRequest("Parameter 'ginNumber' is required.");

            try
            {
                var details = await _generalGoodsIssueService.GetGeneralGinPrintDetailsAsync(ginNumber);
                return Ok(_mapper.Map<GeneralGinPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Goods Issue Note: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-gin/print/pdf?ginNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string ginNumber)
        {
            if (string.IsNullOrWhiteSpace(ginNumber))
                return BadRequest("Parameter 'ginNumber' is required.");

            try
            {
                var details = await _generalGoodsIssueService.GetGeneralGinPrintDetailsAsync(ginNumber);
                byte[] pdfBytes = GeneralGinPrintEngine.GenerateGinPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_GIN_{ginNumber}.pdf");
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
