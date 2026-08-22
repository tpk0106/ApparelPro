using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> K. Estimated Production Schedule - migrated from PR_ESTL2.PRG.
    [Route("api/estimated-production-schedule-report")]
    [ApiController]
    public class EstimatedProductionScheduleReportController : ControllerBase
    {
        private readonly IEstimatedProductionScheduleReportService _estimatedProductionScheduleReportService;
        private readonly IMapper _mapper;

        public EstimatedProductionScheduleReportController(
            IEstimatedProductionScheduleReportService estimatedProductionScheduleReportService, IMapper mapper)
        {
            _estimatedProductionScheduleReportService = estimatedProductionScheduleReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "estimated-production-schedule-report-view")]
        [ProducesResponseType(typeof(EstimatedProductionScheduleReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var report = await _estimatedProductionScheduleReportService.GetReportAsync(fromDate, toDate);
                return Ok(_mapper.Map<EstimatedProductionScheduleReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "estimated-production-schedule-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var report = await _estimatedProductionScheduleReportService.GetReportAsync(fromDate, toDate);
                var pdfBytes = EstimatedProductionScheduleReportEngine.GeneratePdf(report);
                var safeFileName = $"EstimatedProductionSchedule_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
