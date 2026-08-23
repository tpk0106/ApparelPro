using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/stock-movement-reports")]
    [ApiController]
    // GRN/GIN/STRN controllers are all role-gated; a report over the same inventory
    // data gets the same gate (StylewiseReportController currently has none — an
    // oversight there, not a pattern to repeat here).
    [Authorize(Policy = "stock-movement-report")]
    public class StockMovementReportController : ControllerBase
    {
        private readonly IStockMovementReportService _stockMovementReportService;
        private readonly IMapper _mapper;

        public StockMovementReportController(IStockMovementReportService stockMovementReportService, IMapper mapper)
        {
            _stockMovementReportService = stockMovementReportService;
            _mapper = mapper;
        }

        // GET: api/stock-movement-reports/header?buyerCode=2&order=1017-18
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var header = await _stockMovementReportService.GetStockMovementReportHeaderAsync(buyerCode, order);
                return Ok(_mapper.Map<StockMovementReportHeaderAPIModel>(header));
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

        // GET: api/stock-movement-reports/lines?buyerCode=2&order=1017-18&pageSize=25&currentPage=1
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int pageSize,
            [FromQuery] int currentPage,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            var result = await _stockMovementReportService.GetStockMovementReportLinesAsync(
                buyerCode, order, pageSize, currentPage, sortColumn, sortOrder, filterColumn, filterQuery);

            return Ok(_mapper.Map<PaginationAPIModel<StockMovementReportLineAPIModel>>(result));
        }

        // GET: api/stock-movement-reports/pdf?buyerCode=2&order=1017-18
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var header = await _stockMovementReportService.GetStockMovementReportHeaderAsync(buyerCode, order);
                var lines = await _stockMovementReportService.GetStockMovementReportLinesForPdfAsync(buyerCode, order);

                byte[] pdfBytes = StockMovementReportEngine.GenerateStockMovementReportPdf(header, lines);
                string safeFileName = $"StockMovementReport_{buyerCode}_{order.Trim()}_{DateTime.Now:yyyyMMdd}.pdf";
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
