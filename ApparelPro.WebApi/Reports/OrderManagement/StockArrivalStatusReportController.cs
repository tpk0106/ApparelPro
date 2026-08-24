using apparelPro.BusinessLogic.Reports.OrderManagement.StockArrivalStatusReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_STARV.PRG's "STOCK ARRIVAL STATUS REPORT" - Reports -> Order
    // Management -> Stock Arrival Status in this app's nav.
    [Route("api/stock-arrival-status-report")]
    [ApiController]
    public class StockArrivalStatusReportController : ControllerBase
    {
        private readonly IStockArrivalStatusReportService _stockArrivalStatusReportService;
        private readonly IMapper _mapper;

        public StockArrivalStatusReportController(IStockArrivalStatusReportService stockArrivalStatusReportService, IMapper mapper)
        {
            _stockArrivalStatusReportService = stockArrivalStatusReportService;
            _mapper = mapper;
        }

        // GET: api/stock-arrival-status-report/details?buyerCode=&order=&asOfDate=
        [HttpGet("details")]
        [Authorize(Policy = "stock-arrival-status-report")]
        public async Task<IActionResult> GetDetails([FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] DateTime asOfDate)
        {
            try
            {
                var report = await _stockArrivalStatusReportService.GetStockArrivalStatusReportAsync(buyerCode, order, asOfDate);
                var apiModel = _mapper.Map<StockArrivalStatusReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Stock Arrival Status Report: {ex.Message}" });
            }
        }

        // GET: api/stock-arrival-status-report/pdf?buyerCode=&order=&asOfDate=
        [HttpGet("pdf")]
        [Authorize(Policy = "stock-arrival-status-report")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] DateTime asOfDate)
        {
            try
            {
                var report = await _stockArrivalStatusReportService.GetStockArrivalStatusReportAsync(buyerCode, order, asOfDate);
                byte[] pdfBytes = StockArrivalStatusReportEngine.GeneratePdf(report);
                string safeFileName = $"StockArrivalStatusReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Stock Arrival Status Report PDF document: {ex.Message}" });
            }
        }
    }
}
