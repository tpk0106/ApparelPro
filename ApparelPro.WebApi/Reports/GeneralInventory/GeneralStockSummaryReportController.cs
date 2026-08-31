using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.GeneralInventory
{
    [Route("api/general-stock-summary-reports")]
    [ApiController]
    [Authorize(Policy = "general-stock-summary-report")]
    public class GeneralStockSummaryReportController : ControllerBase
    {
        private readonly IGeneralStockSummaryReportService _generalStockSummaryReportService;
        private readonly IMapper _mapper;

        public GeneralStockSummaryReportController(IGeneralStockSummaryReportService generalStockSummaryReportService, IMapper mapper)
        {
            _generalStockSummaryReportService = generalStockSummaryReportService;
            _mapper = mapper;
        }

        // GET: api/general-stock-summary-reports/header?month=12&year=1995&currency1=LKR&currency2=USD
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] int month, [FromQuery] int year, [FromQuery] string currency1, [FromQuery] string currency2)
        {
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");
            if (string.IsNullOrWhiteSpace(currency1))
                return BadRequest("Parameter 'currency1' is required.");
            if (string.IsNullOrWhiteSpace(currency2))
                return BadRequest("Parameter 'currency2' is required.");

            try
            {
                var header = await _generalStockSummaryReportService.GetHeaderAsync(month, year, currency1, currency2);
                return Ok(_mapper.Map<GeneralStockSummaryReportHeaderAPIModel>(header));
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

        // GET: api/general-stock-summary-reports/lines?month=12&year=1995&currency1=LKR&currency2=USD
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] int month, [FromQuery] int year, [FromQuery] string currency1, [FromQuery] string currency2)
        {
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");
            if (string.IsNullOrWhiteSpace(currency1))
                return BadRequest("Parameter 'currency1' is required.");
            if (string.IsNullOrWhiteSpace(currency2))
                return BadRequest("Parameter 'currency2' is required.");

            try
            {
                var lines = await _generalStockSummaryReportService.GetLinesAsync(month, year, currency1, currency2);
                return Ok(_mapper.Map<List<GeneralStockSummaryReportLineAPIModel>>(lines));
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

        // GET: api/general-stock-summary-reports/pdf?month=12&year=1995&currency1=LKR&currency2=USD
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] int month, [FromQuery] int year, [FromQuery] string currency1, [FromQuery] string currency2)
        {
            if (month < 1 || month > 12)
                return BadRequest("Parameter 'month' must be between 1 and 12.");
            if (string.IsNullOrWhiteSpace(currency1))
                return BadRequest("Parameter 'currency1' is required.");
            if (string.IsNullOrWhiteSpace(currency2))
                return BadRequest("Parameter 'currency2' is required.");

            try
            {
                var header = await _generalStockSummaryReportService.GetHeaderAsync(month, year, currency1, currency2);
                var lines = await _generalStockSummaryReportService.GetLinesAsync(month, year, currency1, currency2);

                byte[] pdfBytes = GeneralStockSummaryReportEngine.GenerateStockSummaryReportPdf(header, lines);
                string safeFileName = $"GeneralStockSummaryReport_{year:D4}{month:D2}.pdf";
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
                return StatusCode(500, new { Error = $"Failed to construct Stock Summary Report PDF: {ex.Message}" });
            }
        }
    }
}
