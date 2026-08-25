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
    // endpoint for the item picker (same reuse pattern already used by GeneralGTNController).
    [Route("api/general-inventory-rtn")]
    [ApiController]
    [Authorize(Policy = "general-rtn")]
    public class GeneralRTNController : ControllerBase
    {
        private readonly IGeneralGoodsReturnService _generalGoodsReturnService;
        private readonly IMapper _mapper;

        public GeneralRTNController(IGeneralGoodsReturnService generalGoodsReturnService, IMapper mapper)
        {
            _generalGoodsReturnService = generalGoodsReturnService;
            _mapper = mapper;
        }

        // POST: api/general-inventory-rtn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitGoodsReturn([FromBody] GeneralRTNAPIModel rtnAPIModel)
        {
            if (rtnAPIModel?.Header == null || rtnAPIModel.Lines == null || rtnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GeneralRtnHeaderServiceModel>(rtnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GeneralRtnLineItemServiceModel>>(rtnAPIModel.Lines);

            try
            {
                var success = await _generalGoodsReturnService.CommitGeneralGoodsReturnNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Goods Return Note from Department '{rtnAPIModel.Header.DepartmentCode}' to Stores '{rtnAPIModel.Header.StoreCode}' committed successfully."
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

        // GET: api/general-inventory-rtn/print?rtnNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string rtnNumber)
        {
            if (string.IsNullOrWhiteSpace(rtnNumber))
                return BadRequest("Parameter 'rtnNumber' is required.");

            try
            {
                var details = await _generalGoodsReturnService.GetGeneralRtnPrintDetailsAsync(rtnNumber);
                return Ok(_mapper.Map<GeneralRtnPrintDetailsAPIModel>(details));
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

        // GET: api/general-inventory-rtn/print/pdf?rtnNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string rtnNumber)
        {
            if (string.IsNullOrWhiteSpace(rtnNumber))
                return BadRequest("Parameter 'rtnNumber' is required.");

            try
            {
                var details = await _generalGoodsReturnService.GetGeneralRtnPrintDetailsAsync(rtnNumber);
                byte[] pdfBytes = GeneralRtnPrintEngine.GenerateRtnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_RTN_{rtnNumber}.pdf");
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
