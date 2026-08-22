using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> C. Production Summary (Daily) - migrated from PR_DPROD.PRG.
    [Route("api/production-summary-daily-report")]
    [ApiController]
    public class ProductionSummaryDailyReportController : ControllerBase
    {
        private readonly IProductionSummaryDailyReportService _productionSummaryDailyReportService;
        private readonly IMapper _mapper;

        public ProductionSummaryDailyReportController(
            IProductionSummaryDailyReportService productionSummaryDailyReportService, IMapper mapper)
        {
            _productionSummaryDailyReportService = productionSummaryDailyReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "production-summary-daily-report-view")]
        [ProducesResponseType(typeof(ProductionSummaryDailyReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] DateOnly date)
        {
            try
            {
                var report = await _productionSummaryDailyReportService.GetProductionSummaryDailyReportAsync(date);
                return Ok(_mapper.Map<ProductionSummaryDailyReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "production-summary-daily-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] DateOnly date)
        {
            try
            {
                var report = await _productionSummaryDailyReportService.GetProductionSummaryDailyReportAsync(date);
                var pdfBytes = ProductionSummaryDailyReportEngine.GeneratePdf(report);
                var safeFileName = $"ProductionSummaryDaily_{date:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
