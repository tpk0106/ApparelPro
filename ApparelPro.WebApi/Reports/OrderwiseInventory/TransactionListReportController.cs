using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/orderwise-transaction-list-reports")]
    [ApiController]
    [Authorize(Policy = "orderwise-transaction-list-report")]
    public class TransactionListReportController : ControllerBase
    {
        private readonly ITransactionListReportService _transactionListReportService;
        private readonly IMapper _mapper;

        public TransactionListReportController(ITransactionListReportService transactionListReportService, IMapper mapper)
        {
            _transactionListReportService = transactionListReportService;
            _mapper = mapper;
        }

        // GET: api/orderwise-transaction-list-reports/header?fromDate=1994-03-01&toDate=1994-06-30&transactionType=GR&itemCodePrefix=02
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? transactionType, [FromQuery] string? itemCodePrefix)
        {
            try
            {
                var header = await _transactionListReportService.GetHeaderAsync(fromDate, toDate, transactionType, itemCodePrefix);
                return Ok(_mapper.Map<TransactionListReportHeaderAPIModel>(header));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/orderwise-transaction-list-reports/lines?fromDate=1994-03-01&toDate=1994-06-30&transactionType=GR&itemCodePrefix=02
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? transactionType, [FromQuery] string? itemCodePrefix)
        {
            try
            {
                var lines = await _transactionListReportService.GetLinesAsync(fromDate, toDate, transactionType, itemCodePrefix);
                return Ok(_mapper.Map<List<TransactionListReportLineAPIModel>>(lines));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/orderwise-transaction-list-reports/pdf?fromDate=1994-03-01&toDate=1994-06-30&transactionType=GR&itemCodePrefix=02
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? transactionType, [FromQuery] string? itemCodePrefix)
        {
            try
            {
                var header = await _transactionListReportService.GetHeaderAsync(fromDate, toDate, transactionType, itemCodePrefix);
                var lines = await _transactionListReportService.GetLinesAsync(fromDate, toDate, transactionType, itemCodePrefix);

                byte[] pdfBytes = TransactionListReportEngine.GenerateTransactionListReportPdf(header, lines);
                string safeFileName = $"OrderwiseTransactionList_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct List of Transactions PDF: {ex.Message}" });
            }
        }
    }
}
