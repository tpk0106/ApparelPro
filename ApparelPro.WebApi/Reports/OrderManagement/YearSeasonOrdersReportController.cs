using apparelPro.BusinessLogic.Reports.OrderManagement.YearSeasonOrdersReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_RPO2.PRG's "ORDER CONFIRMATION REPORT" family - Reports -> Order
    // Management -> Year/Season Wise Orders in this app's nav.
    [Route("api/year-season-orders-report")]
    [ApiController]
    public class YearSeasonOrdersReportController : ControllerBase
    {
        private readonly IYearSeasonOrdersReportService _yearSeasonOrdersReportService;
        private readonly IMapper _mapper;

        public YearSeasonOrdersReportController(IYearSeasonOrdersReportService yearSeasonOrdersReportService, IMapper mapper)
        {
            _yearSeasonOrdersReportService = yearSeasonOrdersReportService;
            _mapper = mapper;
        }

        // GET: api/year-season-orders-report/details?year=&season=
        [HttpGet("details")]
        [Authorize(Policy = "year-season-orders-report")]
        public async Task<IActionResult> GetDetails([FromQuery] int? year, [FromQuery] string? season)
        {
            try
            {
                var report = await _yearSeasonOrdersReportService.GetYearSeasonOrdersReportAsync(year, season);
                var apiModel = _mapper.Map<YearSeasonOrdersReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Year/Season Wise Orders Report: {ex.Message}" });
            }
        }

        // GET: api/year-season-orders-report/pdf?year=&season=
        [HttpGet("pdf")]
        [Authorize(Policy = "year-season-orders-report")]
        public async Task<IActionResult> GetPdf([FromQuery] int? year, [FromQuery] string? season)
        {
            try
            {
                var report = await _yearSeasonOrdersReportService.GetYearSeasonOrdersReportAsync(year, season);
                byte[] pdfBytes = YearSeasonOrdersReportEngine.GeneratePdf(report);
                string safeFileName = $"YearSeasonOrdersReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Year/Season Wise Orders Report PDF document: {ex.Message}" });
            }
        }
    }
}
