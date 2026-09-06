using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/item-wise-stock-balance-reports")]
    [ApiController]
    [Authorize(Policy = "item-wise-stock-balance-report")]
    public class ItemWiseStockBalanceController : ControllerBase
    {
        private readonly IItemWiseStockBalanceService _itemWiseStockBalanceService;
        private readonly IMapper _mapper;

        public ItemWiseStockBalanceController(IItemWiseStockBalanceService itemWiseStockBalanceService, IMapper mapper)
        {
            _itemWiseStockBalanceService = itemWiseStockBalanceService;
            _mapper = mapper;
        }

        // GET: api/item-wise-stock-balance-reports/item-search?query=02TI
        // Type-ahead search for the From/To Item range pickers - returns up to 20
        // matching 6-char Stock+Item composite codes with a description each.
        [HttpGet("item-search")]
        public async Task<IActionResult> SearchItemCodes([FromQuery] string query)
        {
            var results = await _itemWiseStockBalanceService.SearchItemCodesAsync(query ?? string.Empty);
            return Ok(_mapper.Map<List<ItemCodeSearchResultAPIModel>>(results));
        }

        // GET: api/item-wise-stock-balance-reports/header?fromRange=02TISS&toRange=02TISS
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] string fromRange, [FromQuery] string toRange)
        {
            if (string.IsNullOrWhiteSpace(fromRange) || string.IsNullOrWhiteSpace(toRange))
                return BadRequest("Parameters 'fromRange' and 'toRange' are required.");

            try
            {
                var header = await _itemWiseStockBalanceService.GetHeaderAsync(fromRange, toRange);
                return Ok(_mapper.Map<ItemWiseStockBalanceHeaderAPIModel>(header));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/item-wise-stock-balance-reports/lines?fromRange=02TISS&toRange=02TISS
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] string fromRange, [FromQuery] string toRange)
        {
            if (string.IsNullOrWhiteSpace(fromRange) || string.IsNullOrWhiteSpace(toRange))
                return BadRequest("Parameters 'fromRange' and 'toRange' are required.");

            try
            {
                var lines = await _itemWiseStockBalanceService.GetLinesAsync(fromRange, toRange);
                return Ok(_mapper.Map<List<ItemWiseStockBalanceLineAPIModel>>(lines));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/item-wise-stock-balance-reports/pdf?fromRange=02TISS&toRange=02TISS
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] string fromRange, [FromQuery] string toRange)
        {
            if (string.IsNullOrWhiteSpace(fromRange) || string.IsNullOrWhiteSpace(toRange))
                return BadRequest("Parameters 'fromRange' and 'toRange' are required.");

            try
            {
                var header = await _itemWiseStockBalanceService.GetHeaderAsync(fromRange, toRange);
                var lines = await _itemWiseStockBalanceService.GetLinesAsync(fromRange, toRange);

                byte[] pdfBytes = ItemWiseStockBalanceEngine.GenerateItemWiseStockBalancePdf(header, lines);
                string safeFileName = $"ItemWiseStockBalances_{fromRange}_{toRange}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Item-wise Stock Balances PDF: {ex.Message}" });
            }
        }
    }
}
