using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.GeneralInventory
{
    [Route("api/general-transaction-list-reports")]
    [ApiController]
    [Authorize(Policy = "general-transaction-list-report")]
    public class GeneralTransactionListReportController : ControllerBase
    {
        private readonly IGeneralTransactionListReportService _generalTransactionListReportService;
        private readonly IMapper _mapper;

        public GeneralTransactionListReportController(IGeneralTransactionListReportService generalTransactionListReportService, IMapper mapper)
        {
            _generalTransactionListReportService = generalTransactionListReportService;
            _mapper = mapper;
        }

        // GET: api/general-transaction-list-reports/header?fromDate=1994-03-01&toDate=1994-03-31&transactionTypeCode=0G&itemCodePrefix=03KABC
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? transactionTypeCode, [FromQuery] string? itemCodePrefix)
        {
            try
            {
                var header = await _generalTransactionListReportService.GetHeaderAsync(fromDate, toDate, transactionTypeCode, itemCodePrefix);
                return Ok(_mapper.Map<GeneralTransactionListReportHeaderAPIModel>(header));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // GET: api/general-transaction-list-reports/lines?fromDate=1994-03-01&toDate=1994-03-31&transactionTypeCode=0G&itemCodePrefix=03KABC
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? transactionTypeCode, [FromQuery] string? itemCodePrefix)
        {
            try
            {
                var lines = await _generalTransactionListReportService.GetLinesAsync(fromDate, toDate, transactionTypeCode, itemCodePrefix);
                return Ok(_mapper.Map<List<GeneralTransactionListReportLineAPIModel>>(lines));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // GET: api/general-transaction-list-reports/pdf?fromDate=1994-03-01&toDate=1994-03-31&transactionTypeCode=0G&itemCodePrefix=03KABC
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? transactionTypeCode, [FromQuery] string? itemCodePrefix)
        {
            try
            {
                var header = await _generalTransactionListReportService.GetHeaderAsync(fromDate, toDate, transactionTypeCode, itemCodePrefix);
                var lines = await _generalTransactionListReportService.GetLinesAsync(fromDate, toDate, transactionTypeCode, itemCodePrefix);

                byte[] pdfBytes = GeneralTransactionListReportEngine.GenerateTransactionListReportPdf(header, lines);
                string safeFileName = $"GeneralTransactionList_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct List of Transactions PDF: {ex.Message}" });
            }
        }
    }
}
