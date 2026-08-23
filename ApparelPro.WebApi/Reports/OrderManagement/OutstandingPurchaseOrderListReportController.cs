using apparelPro.BusinessLogic.Reports.OrderManagement.OutstandingPurchaseOrderListReport;
using apparelPro.BusinessLogic.Services.interfaces.Reports.OrderManagement;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_PLST1.PRG's "LIST OF OUTSTANDING P/O's - Date Wise" report -
    // Reports -> Order Management -> List of Outstanding P/O's.
    [Route("api/outstanding-purchase-order-list-report")]
    [ApiController]
    public class OutstandingPurchaseOrderListReportController : ControllerBase
    {
        private readonly IOutstandingPurchaseOrderListReportService _outstandingPurchaseOrderListReportService;
        private readonly IMapper _mapper;

        public OutstandingPurchaseOrderListReportController(
            IOutstandingPurchaseOrderListReportService outstandingPurchaseOrderListReportService, IMapper mapper)
        {
            _outstandingPurchaseOrderListReportService = outstandingPurchaseOrderListReportService;
            _mapper = mapper;
        }

        // GET: api/outstanding-purchase-order-list-report/details?startDate=&endDate=&basisCode=
        [HttpGet("details")]
        [Authorize(Policy = "outstanding-purchase-order-list-report")]
        public async Task<IActionResult> GetDetails(
            [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string? basisCode)
        {
            if (startDate > endDate)
                return BadRequest("Start Date cannot be after End Date.");

            try
            {
                var report = await _outstandingPurchaseOrderListReportService
                    .GetOutstandingPurchaseOrderListReportAsync(startDate, endDate, basisCode);
                var apiModel = _mapper.Map<OutstandingPurchaseOrderListReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Outstanding P/O List Report: {ex.Message}" });
            }
        }

        // GET: api/outstanding-purchase-order-list-report/pdf?startDate=&endDate=&basisCode=
        [HttpGet("pdf")]
        [Authorize(Policy = "outstanding-purchase-order-list-report")]
        public async Task<IActionResult> GetPdf(
            [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string? basisCode)
        {
            if (startDate > endDate)
                return BadRequest("Start Date cannot be after End Date.");

            try
            {
                var report = await _outstandingPurchaseOrderListReportService
                    .GetOutstandingPurchaseOrderListReportAsync(startDate, endDate, basisCode);
                byte[] pdfBytes = OutstandingPurchaseOrderListReportEngine.GeneratePdf(report);
                string safeFileName = $"OutstandingPOList_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Outstanding P/O List Report PDF document: {ex.Message}" });
            }
        }
    }
}
