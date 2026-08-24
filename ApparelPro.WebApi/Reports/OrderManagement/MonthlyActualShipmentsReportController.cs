using apparelPro.BusinessLogic.Reports.OrderManagement.MonthlyActualShipmentsReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_ACTSP.PRG's "MONTHLY ACTUAL SHIPMENTS" - Reports -> Order Management
    // -> Monthly Actual Shipments in this app's nav.
    [Route("api/monthly-actual-shipments-report")]
    [ApiController]
    public class MonthlyActualShipmentsReportController : ControllerBase
    {
        private readonly IMonthlyActualShipmentsReportService _monthlyActualShipmentsReportService;
        private readonly IMapper _mapper;

        public MonthlyActualShipmentsReportController(IMonthlyActualShipmentsReportService monthlyActualShipmentsReportService, IMapper mapper)
        {
            _monthlyActualShipmentsReportService = monthlyActualShipmentsReportService;
            _mapper = mapper;
        }

        // GET: api/monthly-actual-shipments-report/details?month=&year=
        [HttpGet("details")]
        [Authorize(Policy = "monthly-actual-shipments-report")]
        public async Task<IActionResult> GetDetails([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var report = await _monthlyActualShipmentsReportService.GetMonthlyActualShipmentsReportAsync(month, year);
                var apiModel = _mapper.Map<MonthlyActualShipmentsReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Monthly Actual Shipments Report: {ex.Message}" });
            }
        }

        // GET: api/monthly-actual-shipments-report/pdf?month=&year=
        [HttpGet("pdf")]
        [Authorize(Policy = "monthly-actual-shipments-report")]
        public async Task<IActionResult> GetPdf([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var report = await _monthlyActualShipmentsReportService.GetMonthlyActualShipmentsReportAsync(month, year);
                byte[] pdfBytes = MonthlyActualShipmentsReportEngine.GeneratePdf(report);
                string safeFileName = $"MonthlyActualShipmentsReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Monthly Actual Shipments Report PDF document: {ex.Message}" });
            }
        }
    }
}
