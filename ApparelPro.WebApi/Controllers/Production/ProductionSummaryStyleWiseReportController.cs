using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> D. Production Summary (Style Wise) - migrated from PR_MPRO1.PRG.
    [Route("api/production-summary-style-wise-report")]
    [ApiController]
    public class ProductionSummaryStyleWiseReportController : ControllerBase
    {
        private readonly IProductionSummaryStyleWiseReportService _productionSummaryStyleWiseReportService;
        private readonly IMapper _mapper;

        public ProductionSummaryStyleWiseReportController(
            IProductionSummaryStyleWiseReportService productionSummaryStyleWiseReportService, IMapper mapper)
        {
            _productionSummaryStyleWiseReportService = productionSummaryStyleWiseReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "production-summary-style-wise-report-view")]
        [ProducesResponseType(typeof(ProductionSummaryStyleWiseReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var report = await _productionSummaryStyleWiseReportService.GetReportAsync(startDate, endDate);
                return Ok(_mapper.Map<ProductionSummaryStyleWiseReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "production-summary-style-wise-report-view")]
        public async Task<IActionResult> GetPdfAsync([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var report = await _productionSummaryStyleWiseReportService.GetReportAsync(startDate, endDate);
                var pdfBytes = ProductionSummaryStyleWiseReportEngine.GeneratePdf(report);
                var safeFileName = $"ProductionSummaryStyleWise_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
