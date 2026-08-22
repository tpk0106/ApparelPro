using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> D. Production Summary (Style Wise) - Detailed, the per-line
    // breakdown companion to ProductionSummaryStyleWiseReportController.
    [Route("api/production-summary-style-wise-detailed-report")]
    [ApiController]
    public class ProductionSummaryStyleWiseDetailedReportController : ControllerBase
    {
        private readonly IProductionSummaryStyleWiseDetailedReportService _productionSummaryStyleWiseDetailedReportService;
        private readonly IMapper _mapper;

        public ProductionSummaryStyleWiseDetailedReportController(
            IProductionSummaryStyleWiseDetailedReportService productionSummaryStyleWiseDetailedReportService, IMapper mapper)
        {
            _productionSummaryStyleWiseDetailedReportService = productionSummaryStyleWiseDetailedReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "production-summary-style-wise-detailed-report-view")]
        [ProducesResponseType(typeof(ProductionSummaryStyleWiseDetailedReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var report = await _productionSummaryStyleWiseDetailedReportService.GetReportAsync(startDate, endDate);
                return Ok(_mapper.Map<ProductionSummaryStyleWiseDetailedReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "production-summary-style-wise-detailed-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var report = await _productionSummaryStyleWiseDetailedReportService.GetReportAsync(startDate, endDate);
                var pdfBytes = ProductionSummaryStyleWiseDetailedReportEngine.GeneratePdf(report);
                var safeFileName = $"ProductionSummaryStyleWiseDetailed_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
