using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> J. Production Line Efficiency - migrated from PR_GRH1.PRG.
    [Route("api/line-efficiency-report")]
    [ApiController]
    public class LineEfficiencyReportController : ControllerBase
    {
        private readonly ILineEfficiencyReportService _lineEfficiencyReportService;
        private readonly IMapper _mapper;

        public LineEfficiencyReportController(
            ILineEfficiencyReportService lineEfficiencyReportService, IMapper mapper)
        {
            _lineEfficiencyReportService = lineEfficiencyReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "line-efficiency-report-view")]
        [ProducesResponseType(typeof(LineEfficiencyReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] string lineCode, [FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var report = await _lineEfficiencyReportService.GetReportAsync(lineCode, year, month);
                return Ok(_mapper.Map<LineEfficiencyReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "line-efficiency-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] string lineCode, [FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var report = await _lineEfficiencyReportService.GetReportAsync(lineCode, year, month);
                var pdfBytes = LineEfficiencyReportEngine.GeneratePdf(report);
                var safeFileName = $"LineEfficiency_{lineCode}_{year}_{month:D2}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
