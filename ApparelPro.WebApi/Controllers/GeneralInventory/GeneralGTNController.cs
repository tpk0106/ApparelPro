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
    // endpoint for the item picker rather than duplicating a lookup endpoint here.
    [Route("api/general-inventory-gtn")]
    [ApiController]
    [Authorize(Policy = "general-gtn")]
    public class GeneralGTNController : ControllerBase
    {
        private readonly IGeneralGoodsTransferService _generalGoodsTransferService;
        private readonly IMapper _mapper;

        public GeneralGTNController(IGeneralGoodsTransferService generalGoodsTransferService, IMapper mapper)
        {
            _generalGoodsTransferService = generalGoodsTransferService;
            _mapper = mapper;
        }

        // POST: api/general-inventory-gtn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitGoodsTransfer([FromBody] GeneralGTNAPIModel gtnAPIModel)
        {
            if (gtnAPIModel?.Header == null || gtnAPIModel.Lines == null || gtnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GeneralGtnHeaderServiceModel>(gtnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GeneralGtnLineItemServiceModel>>(gtnAPIModel.Lines);

            try
            {
                var success = await _generalGoodsTransferService.CommitGeneralGoodsTransferNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Transfer Note from '{gtnAPIModel.Header.FromStoreCode}' to '{gtnAPIModel.Header.ToStoreCode}' committed successfully."
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

        // GET: api/general-inventory-gtn/print?gtnNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string gtnNumber)
        {
            if (string.IsNullOrWhiteSpace(gtnNumber))
                return BadRequest("Parameter 'gtnNumber' is required.");

            try
            {
                var details = await _generalGoodsTransferService.GetGeneralGtnPrintDetailsAsync(gtnNumber);
                return Ok(_mapper.Map<GeneralGtnPrintDetailsAPIModel>(details));
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

        // GET: api/general-inventory-gtn/print/pdf?gtnNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string gtnNumber)
        {
            if (string.IsNullOrWhiteSpace(gtnNumber))
                return BadRequest("Parameter 'gtnNumber' is required.");

            try
            {
                var details = await _generalGoodsTransferService.GetGeneralGtnPrintDetailsAsync(gtnNumber);
                byte[] pdfBytes = GeneralGtnPrintEngine.GenerateGtnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_GTN_{gtnNumber}.pdf");
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
