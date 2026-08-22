using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> H. Employee Efficiency (Daily) - migrated from PR_EEF1.PRG.
    [Route("api/daily-employee-efficiency-report")]
    [ApiController]
    public class DailyEmployeeEfficiencyReportController : ControllerBase
    {
        private readonly IDailyEmployeeEfficiencyReportService _dailyEmployeeEfficiencyReportService;
        private readonly IMapper _mapper;

        public DailyEmployeeEfficiencyReportController(
            IDailyEmployeeEfficiencyReportService dailyEmployeeEfficiencyReportService, IMapper mapper)
        {
            _dailyEmployeeEfficiencyReportService = dailyEmployeeEfficiencyReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "daily-employee-efficiency-report-view")]
        [ProducesResponseType(typeof(DailyEmployeeEfficiencyReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] DateOnly date, [FromQuery] string? lineCode)
        {
            try
            {
                var report = await _dailyEmployeeEfficiencyReportService.GetReportAsync(date, lineCode);
                return Ok(_mapper.Map<DailyEmployeeEfficiencyReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "daily-employee-efficiency-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] DateOnly date, [FromQuery] string? lineCode)
        {
            try
            {
                var report = await _dailyEmployeeEfficiencyReportService.GetReportAsync(date, lineCode);
                var pdfBytes = DailyEmployeeEfficiencyReportEngine.GeneratePdf(report);
                var safeFileName = $"DailyEmployeeEfficiency_{date:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
