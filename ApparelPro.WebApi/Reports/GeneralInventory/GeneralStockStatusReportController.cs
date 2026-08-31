using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.GeneralInventory
{
    [Route("api/general-stock-status-reports")]
    [ApiController]
    [Authorize(Policy = "general-stock-status-report")]
    public class GeneralStockStatusReportController : ControllerBase
    {
        private readonly IGeneralStockStatusReportService _generalStockStatusReportService;
        private readonly IMapper _mapper;

        public GeneralStockStatusReportController(IGeneralStockStatusReportService generalStockStatusReportService, IMapper mapper)
        {
            _generalStockStatusReportService = generalStockStatusReportService;
            _mapper = mapper;
        }

        // GET: api/general-stock-status-reports/header?storeCode=M-S&month=8&year=2026
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] string storeCode, [FromQuery] int month, [FromQuery] int year)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");

            try
            {
                var header = await _generalStockStatusReportService.GetHeaderAsync(storeCode, month, year);
                return Ok(_mapper.Map<GeneralStockStatusReportHeaderAPIModel>(header));
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

        // GET: api/general-stock-status-reports/lines?storeCode=M-S&month=8&year=2026
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] string storeCode, [FromQuery] int month, [FromQuery] int year)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");

            var lines = await _generalStockStatusReportService.GetLinesAsync(storeCode, month, year);
            return Ok(_mapper.Map<List<GeneralStockStatusReportLineAPIModel>>(lines));
        }

        // GET: api/general-stock-status-reports/pdf?storeCode=M-S&month=8&year=2026
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] string storeCode, [FromQuery] int month, [FromQuery] int year)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");

            try
            {
                var header = await _generalStockStatusReportService.GetHeaderAsync(storeCode, month, year);
                var lines = await _generalStockStatusReportService.GetLinesAsync(storeCode, month, year);

                byte[] pdfBytes = GeneralStockStatusReportEngine.GenerateStockStatusReportPdf(header, lines);
                string safeFileName = $"GeneralStockStatusReport_{storeCode.Trim()}_{year:D4}{month:D2}.pdf";
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
                return StatusCode(500, new { Error = $"Failed to construct Stock Status Report PDF: {ex.Message}" });
            }
        }
    }
}
