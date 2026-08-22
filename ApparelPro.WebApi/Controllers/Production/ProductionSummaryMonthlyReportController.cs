using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> B. Production Summary (Monthly) - migrated from PR_MPROD.PRG.
    [Route("api/production-summary-monthly-report")]
    [ApiController]
    public class ProductionSummaryMonthlyReportController : ControllerBase
    {
        private readonly IProductionSummaryMonthlyReportService _productionSummaryMonthlyReportService;
        private readonly IMapper _mapper;

        public ProductionSummaryMonthlyReportController(
            IProductionSummaryMonthlyReportService productionSummaryMonthlyReportService, IMapper mapper)
        {
            _productionSummaryMonthlyReportService = productionSummaryMonthlyReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "production-summary-monthly-report-view")]
        [ProducesResponseType(typeof(ProductionSummaryMonthlyReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var report = await _productionSummaryMonthlyReportService.GetReportAsync(year, month);
                return Ok(_mapper.Map<ProductionSummaryMonthlyReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "production-summary-monthly-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var report = await _productionSummaryMonthlyReportService.GetReportAsync(year, month);
                var pdfBytes = ProductionSummaryMonthlyReportEngine.GeneratePdf(report);
                var safeFileName = $"ProductionSummaryMonthly_{year}_{month:00}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
