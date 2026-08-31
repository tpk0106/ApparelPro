using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/stock-valuation-monthly-reports")]
    [ApiController]
    [Authorize(Policy = "stock-valuation-monthly-report")]
    public class StockValuationMonthlyReportController : ControllerBase
    {
        private readonly IStockValuationMonthlyReportService _stockValuationMonthlyReportService;
        private readonly IMapper _mapper;

        public StockValuationMonthlyReportController(IStockValuationMonthlyReportService stockValuationMonthlyReportService, IMapper mapper)
        {
            _stockValuationMonthlyReportService = stockValuationMonthlyReportService;
            _mapper = mapper;
        }

        // GET: api/stock-valuation-monthly-reports/header?fromDate=1994-03-01&toDate=1994-06-30
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var header = await _stockValuationMonthlyReportService.GetHeaderAsync(fromDate, toDate);
                return Ok(_mapper.Map<StockValuationMonthlyReportHeaderAPIModel>(header));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/stock-valuation-monthly-reports/lines?fromDate=1994-03-01&toDate=1994-06-30
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var lines = await _stockValuationMonthlyReportService.GetLinesAsync(fromDate, toDate);
                return Ok(_mapper.Map<List<StockValuationMonthlyReportLineAPIModel>>(lines));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/stock-valuation-monthly-reports/pdf?fromDate=1994-03-01&toDate=1994-06-30
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var header = await _stockValuationMonthlyReportService.GetHeaderAsync(fromDate, toDate);
                var lines = await _stockValuationMonthlyReportService.GetLinesAsync(fromDate, toDate);

                byte[] pdfBytes = StockValuationMonthlyReportEngine.GenerateStockValuationMonthlyReportPdf(header, lines);
                string safeFileName = $"StockValuationMonthlyReport_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Stock Valuation Report (Monthly) PDF: {ex.Message}" });
            }
        }
    }
}
