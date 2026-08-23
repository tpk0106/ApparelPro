using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/stock-movement-item-reports")]
    [ApiController]
    [Authorize(Policy = "stock-movement-item-report")]
    public class StockMovementItemReportController : ControllerBase
    {
        private readonly IStockMovementItemReportService _stockMovementItemReportService;
        private readonly IMapper _mapper;

        public StockMovementItemReportController(IStockMovementItemReportService stockMovementItemReportService, IMapper mapper)
        {
            _stockMovementItemReportService = stockMovementItemReportService;
            _mapper = mapper;
        }

        // GET: api/stock-movement-item-reports/items?buyerCode=2&order=1017-18
        // Step 3 of the Buyer -> Order -> Item cascade.
        [HttpGet("items")]
        public async Task<IActionResult> GetItems([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            var items = await _stockMovementItemReportService.GetAvailableItemsAsync(buyerCode, order);
            return Ok(_mapper.Map<List<StockMovementItemOptionAPIModel>>(items));
        }

        // GET: api/stock-movement-item-reports/header?buyerCode=2&order=1017-18&itemCode=ABC123
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] string itemCode)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");
            if (string.IsNullOrWhiteSpace(itemCode))
                return BadRequest("Parameter 'itemCode' is required.");

            try
            {
                var header = await _stockMovementItemReportService.GetHeaderAsync(buyerCode, order, itemCode);
                return Ok(_mapper.Map<StockMovementItemReportHeaderAPIModel>(header));
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

        // GET: api/stock-movement-item-reports/lines?buyerCode=2&order=1017-18&itemCode=ABC123&pageSize=25&currentPage=1
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] string itemCode,
            [FromQuery] int pageSize,
            [FromQuery] int currentPage)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");
            if (string.IsNullOrWhiteSpace(itemCode))
                return BadRequest("Parameter 'itemCode' is required.");

            var result = await _stockMovementItemReportService.GetLinesAsync(buyerCode, order, itemCode, pageSize, currentPage);
            return Ok(_mapper.Map<PaginationAPIModel<StockMovementItemReportLineAPIModel>>(result));
        }

        // GET: api/stock-movement-item-reports/pdf?buyerCode=2&order=1017-18&itemCode=ABC123
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] string itemCode)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");
            if (string.IsNullOrWhiteSpace(itemCode))
                return BadRequest("Parameter 'itemCode' is required.");

            try
            {
                var header = await _stockMovementItemReportService.GetHeaderAsync(buyerCode, order, itemCode);
                var lines = await _stockMovementItemReportService.GetLinesForPdfAsync(buyerCode, order, itemCode);

                byte[] pdfBytes = StockMovementItemReportEngine.GenerateStockMovementItemReportPdf(header, lines);
                string safeFileName = $"StockMovementItemReport_{buyerCode}_{order.Trim()}_{itemCode.Trim()}_{DateTime.Now:yyyyMMdd}.pdf";
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
                return StatusCode(500, new { Error = $"Failed to construct Stock Movement Item Report PDF: {ex.Message}" });
            }
        }
    }
}
