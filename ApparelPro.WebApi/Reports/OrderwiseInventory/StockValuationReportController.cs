using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/stock-valuation-reports")]
    [ApiController]
    [Authorize(Policy = "stock-valuation-report")]
    public class StockValuationReportController : ControllerBase
    {
        private readonly IStockValuationReportService _stockValuationReportService;
        private readonly IMapper _mapper;

        public StockValuationReportController(IStockValuationReportService stockValuationReportService, IMapper mapper)
        {
            _stockValuationReportService = stockValuationReportService;
            _mapper = mapper;
        }

        // GET: api/stock-valuation-reports/header?buyerCode=2&order=1017-18
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var header = await _stockValuationReportService.GetHeaderAsync(buyerCode, order);
                return Ok(_mapper.Map<StockValuationReportHeaderAPIModel>(header));
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

        // GET: api/stock-valuation-reports/lines?buyerCode=2&order=1017-18
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var lines = await _stockValuationReportService.GetLinesAsync(buyerCode, order);
                return Ok(_mapper.Map<List<StockValuationReportLineAPIModel>>(lines));
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

        // GET: api/stock-valuation-reports/pdf?buyerCode=2&order=1017-18
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var header = await _stockValuationReportService.GetHeaderAsync(buyerCode, order);
                var lines = await _stockValuationReportService.GetLinesAsync(buyerCode, order);

                byte[] pdfBytes = StockValuationReportEngine.GenerateStockValuationReportPdf(header, lines);
                string safeFileName = $"StockValuationReport_{buyerCode}_{order}.pdf";
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
                return StatusCode(500, new { Error = $"Failed to construct Stock Valuation Report PDF: {ex.Message}" });
            }
        }
    }
}
