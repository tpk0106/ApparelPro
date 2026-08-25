using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.GeneralInventory
{
    [Route("api/general-inventory-po")]
    [ApiController]
    [Authorize(Policy = "general-po")]
    public class GeneralPOController : ControllerBase
    {
        private readonly IGeneralPurchaseOrderService _generalPurchaseOrderService;
        private readonly IMapper _mapper;

        public GeneralPOController(IGeneralPurchaseOrderService generalPurchaseOrderService, IMapper mapper)
        {
            _generalPurchaseOrderService = generalPurchaseOrderService;
            _mapper = mapper;
        }

        // POST: api/general-inventory-po/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitPurchaseOrder([FromBody] GeneralPOAPIModel poAPIModel)
        {
            if (poAPIModel?.Header == null || poAPIModel.Lines == null || poAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing header or line items.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<GeneralPoHeaderServiceModel>(poAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<GeneralPoLineItemServiceModel>>(poAPIModel.Lines);

            try
            {
                var result = await _generalPurchaseOrderService.CommitGeneralPurchaseOrderAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(_mapper.Map<GeneralPoCommitResultAPIModel>(result));
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

        // GET: api/general-inventory-po/000001
        [HttpGet("{poNumber}")]
        public async Task<IActionResult> GetPurchaseOrder(string poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return BadRequest("Parameter 'poNumber' is required.");

            try
            {
                var result = await _generalPurchaseOrderService.GetGeneralPurchaseOrderAsync(poNumber);
                return Ok(_mapper.Map<GeneralPOAPIModel>(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Purchase Order: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-po/print?poNumber=000001
        [HttpGet("print")]
        public async Task<IActionResult> GetPrintDetails([FromQuery] string poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return BadRequest("Parameter 'poNumber' is required.");

            try
            {
                var details = await _generalPurchaseOrderService.GetGeneralPoPrintDetailsAsync(poNumber);
                return Ok(_mapper.Map<GeneralPoPrintDetailsAPIModel>(details));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load Purchase Order: {ex.Message}" });
            }
        }

        // GET: api/general-inventory-po/print/pdf?poNumber=000001
        [HttpGet("print/pdf")]
        public async Task<IActionResult> GetPrintPdf([FromQuery] string poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return BadRequest("Parameter 'poNumber' is required.");

            try
            {
                var details = await _generalPurchaseOrderService.GetGeneralPoPrintDetailsAsync(poNumber);
                byte[] pdfBytes = GeneralPoPrintEngine.GeneratePoPrintPdf(details);
                return File(pdfBytes, "application/pdf", $"General_PO_{poNumber}.pdf");
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
