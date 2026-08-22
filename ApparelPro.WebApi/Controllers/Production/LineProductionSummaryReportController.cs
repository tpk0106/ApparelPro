using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> E. Line Production Summary - migrated from PR_LPROD.PRG.
    [Route("api/line-production-summary-report")]
    [ApiController]
    public class LineProductionSummaryReportController : ControllerBase
    {
        private readonly ILineProductionSummaryReportService _lineProductionSummaryReportService;
        private readonly IMapper _mapper;

        public LineProductionSummaryReportController(
            ILineProductionSummaryReportService lineProductionSummaryReportService, IMapper mapper)
        {
            _lineProductionSummaryReportService = lineProductionSummaryReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "line-production-summary-report-view")]
        [ProducesResponseType(typeof(LineProductionSummaryReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var report = await _lineProductionSummaryReportService.GetReportAsync(startDate, endDate);
                return Ok(_mapper.Map<LineProductionSummaryReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "line-production-summary-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var report = await _lineProductionSummaryReportService.GetReportAsync(startDate, endDate);
                var pdfBytes = LineProductionSummaryReportEngine.GeneratePdf(report);
                var safeFileName = $"LineProductionSummary_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
