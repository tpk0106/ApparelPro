using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/orderwise-stock-status-reports")]
    [ApiController]
    [Authorize(Policy = "orderwise-stock-status-report")]
    public class StockStatusReportController : ControllerBase
    {
        private readonly IStockStatusReportService _stockStatusReportService;
        private readonly IMapper _mapper;

        public StockStatusReportController(IStockStatusReportService stockStatusReportService, IMapper mapper)
        {
            _stockStatusReportService = stockStatusReportService;
            _mapper = mapper;
        }

        // GET: api/orderwise-stock-status-reports/header?buyerCode=2&order=1017-18
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var header = await _stockStatusReportService.GetHeaderAsync(buyerCode, order);
                return Ok(_mapper.Map<StockStatusReportHeaderAPIModel>(header));
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

        // GET: api/orderwise-stock-status-reports/lines?buyerCode=2&order=1017-18
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var lines = await _stockStatusReportService.GetLinesAsync(buyerCode, order);
                return Ok(_mapper.Map<List<StockStatusReportLineAPIModel>>(lines));
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

        // GET: api/orderwise-stock-status-reports/pdf?buyerCode=2&order=1017-18
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var header = await _stockStatusReportService.GetHeaderAsync(buyerCode, order);
                var lines = await _stockStatusReportService.GetLinesAsync(buyerCode, order);

                byte[] pdfBytes = StockStatusReportEngine.GenerateStockStatusReportPdf(header, lines);
                string safeFileName = $"OrderwiseStockStatusReport_{buyerCode}_{order}.pdf";
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
