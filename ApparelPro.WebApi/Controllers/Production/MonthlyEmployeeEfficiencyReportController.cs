using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> I. Employee Efficiency (Monthly) - migrated from PR_REP4.PRG.
    [Route("api/monthly-employee-efficiency-report")]
    [ApiController]
    public class MonthlyEmployeeEfficiencyReportController : ControllerBase
    {
        private readonly IMonthlyEmployeeEfficiencyReportService _monthlyEmployeeEfficiencyReportService;
        private readonly IMapper _mapper;

        public MonthlyEmployeeEfficiencyReportController(
            IMonthlyEmployeeEfficiencyReportService monthlyEmployeeEfficiencyReportService, IMapper mapper)
        {
            _monthlyEmployeeEfficiencyReportService = monthlyEmployeeEfficiencyReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "monthly-employee-efficiency-report-view")]
        [ProducesResponseType(typeof(MonthlyEmployeeEfficiencyReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var report = await _monthlyEmployeeEfficiencyReportService.GetReportAsync(year, month);
                return Ok(_mapper.Map<MonthlyEmployeeEfficiencyReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "monthly-employee-efficiency-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var report = await _monthlyEmployeeEfficiencyReportService.GetReportAsync(year, month);
                var pdfBytes = MonthlyEmployeeEfficiencyReportEngine.GeneratePdf(report);
                var safeFileName = $"MonthlyEmployeeEfficiency_{year}_{month:D2}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
