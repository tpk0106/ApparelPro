using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.GeneralInventory
{
    [Route("api/general-inventory-strn")]
    [ApiController]
    [Authorize(Policy = "general-strn")]
    public class GeneralSTRNController : ControllerBase
    {
        private readonly IGeneralStoresRequisitionService _generalStoresRequisitionService;
        private readonly IMapper _mapper;

        public GeneralSTRNController(IGeneralStoresRequisitionService generalStoresRequisitionService, IMapper mapper)
        {
            _generalStoresRequisitionService = generalStoresRequisitionService;
            _mapper = mapper;
        }

        // GET: api/general-inventory-strn/verify-stock?storeCode=M-S&itemCode=02BT&targetUnit=PCS
        [HttpGet("verify-stock")]
        public async Task<IActionResult> VerifyStock([FromQuery] string storeCode, [FromQuery] string itemCode, [FromQuery] string targetUnit)
        {
            if (string.IsNullOrEmpty(storeCode) || string.IsNullOrEmpty(itemCode))
                return BadRequest("Parameters 'storeCode' and 'itemCode' cannot be empty.");

            try
            {
                var availability = await _generalStoresRequisitionService.VerifyStockItemAvailabilityAsync(storeCode, itemCode, targetUnit);
                if (availability == null)
                    return NotFound("The requested item could not be found in the store's stock master.");

                return Ok(_mapper.Map<GeneralStockItemAvailabilityAPIModel>(availability));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to read live item balance: {ex.Message}" });
            }
        }

        // POST: api/general-inventory-strn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitRequisition([FromBody] GeneralSTRNAPIModel strnAPIModel)
        {
            if (strnAPIModel?.Header == null || strnAPIModel.Lines == null || strnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";
            var strnServiceModel = _mapper.Map<GeneralSTRNServiceModel>(strnAPIModel);

            try
            {
                var success = await _generalStoresRequisitionService.CommitGeneralStoresRequisitionNoteAsync(
                    strnServiceModel.Header, strnServiceModel.Lines, activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Stores Requisition Note '{strnServiceModel.Header.SrnNumber}' committed successfully."
                });
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

        // GET: api/general-inventory-strn/available-choices?storeCode=M-S
        [HttpGet("available-choices")]
        public async Task<IActionResult> GetAvailableChoices([FromQuery] string storeCode)
        {
            if (string.IsNullOrEmpty(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");

            try
            {
                var choices = await _generalStoresRequisitionService.GetAvailableStockChoicesAsync(storeCode);
                return Ok(_mapper.Map<List<GeneralStockLookupRowAPIModel>>(choices));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to build stock search options: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-strn/print?srnNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string srnNumber)
        {
            if (string.IsNullOrWhiteSpace(srnNumber))
                return BadRequest("Parameter 'srnNumber' is required.");

            try
            {
                var details = await _generalStoresRequisitionService.GetGeneralStrnPrintDetailsAsync(srnNumber);
                return Ok(_mapper.Map<GeneralStrnPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Stores Requisition Note: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-strn/stores
        [HttpGet("stores")]
        public async Task<IActionResult> GetStores()
        {
            try
            {
                var stores = await _generalStoresRequisitionService.GetStoresAsync();
                return Ok(_mapper.Map<List<GeneralStoreAPIModel>>(stores));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load stores: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-strn/print/pdf?srnNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string srnNumber)
        {
            if (string.IsNullOrWhiteSpace(srnNumber))
                return BadRequest("Parameter 'srnNumber' is required.");

            try
            {
                var details = await _generalStoresRequisitionService.GetGeneralStrnPrintDetailsAsync(srnNumber);
                byte[] pdfBytes = GeneralStrnPrintEngine.GenerateStrnPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_SRN_{srnNumber}.pdf");
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
