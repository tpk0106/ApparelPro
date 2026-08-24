using apparelPro.BusinessLogic.Reports.OrderManagement.PendingEventsReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_EVPND.PRG's "PENDING EVENTS" - Reports -> Order Management -> Pending
    // Events in this app's nav.
    [Route("api/pending-events-report")]
    [ApiController]
    public class PendingEventsReportController : ControllerBase
    {
        private readonly IPendingEventsReportService _pendingEventsReportService;
        private readonly IMapper _mapper;

        public PendingEventsReportController(IPendingEventsReportService pendingEventsReportService, IMapper mapper)
        {
            _pendingEventsReportService = pendingEventsReportService;
            _mapper = mapper;
        }

        // GET: api/pending-events-report/details?asOfDate=
        [HttpGet("details")]
        [Authorize(Policy = "pending-events-report")]
        public async Task<IActionResult> GetDetails([FromQuery] DateTime asOfDate)
        {
            try
            {
                var report = await _pendingEventsReportService.GetPendingEventsReportAsync(asOfDate);
                var apiModel = _mapper.Map<PendingEventsReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Pending Events Report: {ex.Message}" });
            }
        }

        // GET: api/pending-events-report/pdf?asOfDate=
        [HttpGet("pdf")]
        [Authorize(Policy = "pending-events-report")]
        public async Task<IActionResult> GetPdf([FromQuery] DateTime asOfDate)
        {
            try
            {
                var report = await _pendingEventsReportService.GetPendingEventsReportAsync(asOfDate);
                byte[] pdfBytes = PendingEventsReportEngine.GeneratePdf(report);
                string safeFileName = $"PendingEventsReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Pending Events Report PDF document: {ex.Message}" });
            }
        }
    }
}
