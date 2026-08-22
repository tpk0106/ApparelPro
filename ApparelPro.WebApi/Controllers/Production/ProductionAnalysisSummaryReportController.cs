using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> L. Production Analysis Summary (for Style) - migrated
    // from PR_MPRO2.PRG.
    [Route("api/production-analysis-summary-report")]
    [ApiController]
    public class ProductionAnalysisSummaryReportController : ControllerBase
    {
        private readonly IProductionAnalysisSummaryReportService _productionAnalysisSummaryReportService;
        private readonly IMapper _mapper;

        public ProductionAnalysisSummaryReportController(
            IProductionAnalysisSummaryReportService productionAnalysisSummaryReportService, IMapper mapper)
        {
            _productionAnalysisSummaryReportService = productionAnalysisSummaryReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "production-analysis-summary-report-view")]
        [ProducesResponseType(typeof(ProductionAnalysisSummaryReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            try
            {
                var report = await _productionAnalysisSummaryReportService.GetReportAsync(buyerCode, order, typeCode, styleCode);
                return Ok(_mapper.Map<ProductionAnalysisSummaryReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "production-analysis-summary-report-view")]
        public async Task<IActionResult> GetPdfAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            try
            {
                var report = await _productionAnalysisSummaryReportService.GetReportAsync(buyerCode, order, typeCode, styleCode);
                var pdfBytes = ProductionAnalysisSummaryReportEngine.GeneratePdf(report);
                var safeFileName = $"ProductionAnalysisSummary_{buyerCode}_{order}_{styleCode}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
