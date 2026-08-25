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
    // endpoint for the item picker (same reuse pattern as GeneralGTNController/GeneralRTNController).
    [Route("api/general-inventory-dgn")]
    [ApiController]
    [Authorize(Policy = "general-dgn")]
    public class GeneralDGNController : ControllerBase
    {
        private readonly IGeneralDamagedGoodsService _generalDamagedGoodsService;
        private readonly IMapper _mapper;

        public GeneralDGNController(IGeneralDamagedGoodsService generalDamagedGoodsService, IMapper mapper)
        {
            _generalDamagedGoodsService = generalDamagedGoodsService;
            _mapper = mapper;
        }

        // POST: api/general-inventory-dgn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitDamagedGoods([FromBody] GeneralDGNAPIModel dgnAPIModel)
        {
            if (dgnAPIModel?.Header == null || dgnAPIModel.Lines == null || dgnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GeneralDgnHeaderServiceModel>(dgnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GeneralDgnLineItemServiceModel>>(dgnAPIModel.Lines);

            try
            {
                var success = await _generalDamagedGoodsService.CommitGeneralDamagedGoodsNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Damaged Goods Note for Stores '{dgnAPIModel.Header.StoreCode}' committed successfully."
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

        // GET: api/general-inventory-dgn/print?dgnNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string dgnNumber)
        {
            if (string.IsNullOrWhiteSpace(dgnNumber))
                return BadRequest("Parameter 'dgnNumber' is required.");

            try
            {
                var details = await _generalDamagedGoodsService.GetGeneralDgnPrintDetailsAsync(dgnNumber);
                return Ok(_mapper.Map<GeneralDgnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Damaged Goods Note: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-dgn/print/pdf?dgnNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string dgnNumber)
        {
            if (string.IsNullOrWhiteSpace(dgnNumber))
                return BadRequest("Parameter 'dgnNumber' is required.");

            try
            {
                var details = await _generalDamagedGoodsService.GetGeneralDgnPrintDetailsAsync(dgnNumber);
                byte[] pdfBytes = GeneralDgnPrintEngine.GenerateDgnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_DGN_{dgnNumber}.pdf");
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
