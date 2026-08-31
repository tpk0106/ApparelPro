using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/orderwise-stock-summary-reports")]
    [ApiController]
    [Authorize(Policy = "orderwise-stock-summary-report")]
    public class StockSummaryReportController : ControllerBase
    {
        private readonly IStockSummaryReportService _stockSummaryReportService;
        private readonly IMapper _mapper;

        public StockSummaryReportController(IStockSummaryReportService stockSummaryReportService, IMapper mapper)
        {
            _stockSummaryReportService = stockSummaryReportService;
            _mapper = mapper;
        }

        // GET: api/orderwise-stock-summary-reports/header?currency1=LKR&currency2=USD
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] string currency1, [FromQuery] string currency2)
        {
            if (string.IsNullOrWhiteSpace(currency1))
                return BadRequest("Parameter 'currency1' is required.");
            if (string.IsNullOrWhiteSpace(currency2))
                return BadRequest("Parameter 'currency2' is required.");

            try
            {
                var header = await _stockSummaryReportService.GetHeaderAsync(currency1, currency2);
                return Ok(_mapper.Map<StockSummaryReportHeaderAPIModel>(header));
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

        // GET: api/orderwise-stock-summary-reports/lines?currency1=LKR&currency2=USD
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] string currency1, [FromQuery] string currency2)
        {
            if (string.IsNullOrWhiteSpace(currency1))
                return BadRequest("Parameter 'currency1' is required.");
            if (string.IsNullOrWhiteSpace(currency2))
                return BadRequest("Parameter 'currency2' is required.");

            try
            {
                var lines = await _stockSummaryReportService.GetLinesAsync(currency1, currency2);
                return Ok(_mapper.Map<List<StockSummaryReportLineAPIModel>>(lines));
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

        // GET: api/orderwise-stock-summary-reports/pdf?currency1=LKR&currency2=USD
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] string currency1, [FromQuery] string currency2)
        {
            if (string.IsNullOrWhiteSpace(currency1))
                return BadRequest("Parameter 'currency1' is required.");
            if (string.IsNullOrWhiteSpace(currency2))
                return BadRequest("Parameter 'currency2' is required.");

            try
            {
                var header = await _stockSummaryReportService.GetHeaderAsync(currency1, currency2);
                var lines = await _stockSummaryReportService.GetLinesAsync(currency1, currency2);

                byte[] pdfBytes = StockSummaryReportEngine.GenerateStockSummaryReportPdf(header, lines);
                string safeFileName = $"OrderwiseStockSummaryReport_{currency1}_{currency2}.pdf";
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
