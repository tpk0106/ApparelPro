using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> A. Production Schedule - migrated from PR_MSCHD.PRG.
    [Route("api/production-schedule-report")]
    [ApiController]
    public class ProductionScheduleReportController : ControllerBase
    {
        private readonly IProductionScheduleReportService _productionScheduleReportService;
        private readonly IMapper _mapper;

        public ProductionScheduleReportController(
            IProductionScheduleReportService productionScheduleReportService, IMapper mapper)
        {
            _productionScheduleReportService = productionScheduleReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "production-schedule-report-view")]
        [ProducesResponseType(typeof(ProductionScheduleReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var report = await _productionScheduleReportService.GetProductionScheduleReportAsync(fromDate, toDate);
                return Ok(_mapper.Map<ProductionScheduleReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "production-schedule-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var report = await _productionScheduleReportService.GetProductionScheduleReportAsync(fromDate, toDate);
                var pdfBytes = ProductionScheduleReportEngine.GeneratePdf(report);
                var safeFileName = $"ProductionSchedule_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
