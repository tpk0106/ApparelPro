using apparelPro.BusinessLogic.Reports.OrderManagement.ScheduledShipmentsReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_RSHP1.PRG's "SCHEDULE SHIPMENT DETAIL REPORT" - Reports -> Order
    // Management -> Scheduled Shipments in this app's nav.
    [Route("api/scheduled-shipments-report")]
    [ApiController]
    public class ScheduledShipmentsReportController : ControllerBase
    {
        private readonly IScheduledShipmentsReportService _scheduledShipmentsReportService;
        private readonly IMapper _mapper;

        public ScheduledShipmentsReportController(IScheduledShipmentsReportService scheduledShipmentsReportService, IMapper mapper)
        {
            _scheduledShipmentsReportService = scheduledShipmentsReportService;
            _mapper = mapper;
        }

        // GET: api/scheduled-shipments-report/details?buyerCode=&order=
        [HttpGet("details")]
        [Authorize(Policy = "scheduled-shipments-report")]
        public async Task<IActionResult> GetDetails([FromQuery] int? buyerCode, [FromQuery] string? order)
        {
            try
            {
                var report = await _scheduledShipmentsReportService.GetScheduledShipmentsReportAsync(buyerCode, order);
                var apiModel = _mapper.Map<ScheduledShipmentsReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Scheduled Shipments Report: {ex.Message}" });
            }
        }

        // GET: api/scheduled-shipments-report/pdf?buyerCode=&order=
        [HttpGet("pdf")]
        [Authorize(Policy = "scheduled-shipments-report")]
        public async Task<IActionResult> GetPdf([FromQuery] int? buyerCode, [FromQuery] string? order)
        {
            try
            {
                var report = await _scheduledShipmentsReportService.GetScheduledShipmentsReportAsync(buyerCode, order);
                byte[] pdfBytes = ScheduledShipmentsReportEngine.GeneratePdf(report);
                string safeFileName = $"ScheduledShipmentsReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Scheduled Shipments Report PDF document: {ex.Message}" });
            }
        }
    }
}
