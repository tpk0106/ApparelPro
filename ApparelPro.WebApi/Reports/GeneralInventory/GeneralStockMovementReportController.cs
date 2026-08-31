using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.GeneralInventory
{
    [Route("api/general-stock-movement-reports")]
    [ApiController]
    [Authorize(Policy = "general-stock-movement-report")]
    public class GeneralStockMovementReportController : ControllerBase
    {
        private readonly IGeneralStockMovementReportService _generalStockMovementReportService;
        private readonly IMapper _mapper;

        public GeneralStockMovementReportController(IGeneralStockMovementReportService generalStockMovementReportService, IMapper mapper)
        {
            _generalStockMovementReportService = generalStockMovementReportService;
            _mapper = mapper;
        }

        // GET: api/general-stock-movement-reports/header?storeCode=M-S&itemCode=...&month=12&year=1994
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] string storeCode, [FromQuery] string itemCode, [FromQuery] int month, [FromQuery] int year)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (string.IsNullOrWhiteSpace(itemCode))
                return BadRequest("Parameter 'itemCode' is required.");
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");

            try
            {
                var header = await _generalStockMovementReportService.GetHeaderAsync(storeCode, itemCode, month, year);
                return Ok(_mapper.Map<GeneralStockMovementReportHeaderAPIModel>(header));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/general-stock-movement-reports/lines?storeCode=M-S&itemCode=...&month=12&year=1994
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] string storeCode, [FromQuery] string itemCode, [FromQuery] int month, [FromQuery] int year)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (string.IsNullOrWhiteSpace(itemCode))
                return BadRequest("Parameter 'itemCode' is required.");
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");

            try
            {
                var lines = await _generalStockMovementReportService.GetLinesAsync(storeCode, itemCode, month, year);
                return Ok(_mapper.Map<List<GeneralStockMovementReportLineAPIModel>>(lines));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/general-stock-movement-reports/pdf?storeCode=M-S&itemCode=...&month=12&year=1994
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] string storeCode, [FromQuery] string itemCode, [FromQuery] int month, [FromQuery] int year)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (string.IsNullOrWhiteSpace(itemCode))
                return BadRequest("Parameter 'itemCode' is required.");
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");

            try
            {
                var header = await _generalStockMovementReportService.GetHeaderAsync(storeCode, itemCode, month, year);
                var lines = await _generalStockMovementReportService.GetLinesAsync(storeCode, itemCode, month, year);

                byte[] pdfBytes = GeneralStockMovementReportEngine.GenerateStockMovementReportPdf(header, lines);
                string safeFileName = $"GeneralStockMovementReport_{storeCode.Trim()}_{itemCode.Trim()}_{year:D4}{month:D2}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Stock Movement Report PDF: {ex.Message}" });
            }
        }
    }
}
