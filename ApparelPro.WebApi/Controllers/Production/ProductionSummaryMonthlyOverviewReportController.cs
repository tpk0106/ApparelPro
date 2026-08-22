using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> B2. Production Summary (Monthly) - Simplified. Not a
    // legacy screen - see ProductionSummaryMonthlyOverviewReportServiceModel.
    [Route("api/production-summary-monthly-overview-report")]
    [ApiController]
    public class ProductionSummaryMonthlyOverviewReportController : ControllerBase
    {
        private readonly IProductionSummaryMonthlyOverviewReportService _productionSummaryMonthlyOverviewReportService;
        private readonly IMapper _mapper;

        public ProductionSummaryMonthlyOverviewReportController(
            IProductionSummaryMonthlyOverviewReportService productionSummaryMonthlyOverviewReportService, IMapper mapper)
        {
            _productionSummaryMonthlyOverviewReportService = productionSummaryMonthlyOverviewReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "production-summary-monthly-overview-report-view")]
        [ProducesResponseType(typeof(ProductionSummaryMonthlyOverviewReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var report = await _productionSummaryMonthlyOverviewReportService.GetReportAsync(year, month);
                return Ok(_mapper.Map<ProductionSummaryMonthlyOverviewReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "production-summary-monthly-overview-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var report = await _productionSummaryMonthlyOverviewReportService.GetReportAsync(year, month);
                var pdfBytes = ProductionSummaryMonthlyOverviewReportEngine.GeneratePdf(report);
                var safeFileName = $"ProductionSummaryMonthlyOverview_{year}_{month:00}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
