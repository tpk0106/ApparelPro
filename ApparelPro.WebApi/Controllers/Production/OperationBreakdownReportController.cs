using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> F. Operation Breakdown - migrated from PR_REP1.PRG.
    [Route("api/operation-breakdown-report")]
    [ApiController]
    public class OperationBreakdownReportController : ControllerBase
    {
        private readonly IOperationBreakdownReportService _operationBreakdownReportService;
        private readonly IMapper _mapper;

        public OperationBreakdownReportController(
            IOperationBreakdownReportService operationBreakdownReportService, IMapper mapper)
        {
            _operationBreakdownReportService = operationBreakdownReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "operation-breakdown-report-view")]
        [ProducesResponseType(typeof(OperationBreakdownReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            try
            {
                var report = await _operationBreakdownReportService.GetReportAsync(buyerCode, order, typeCode, styleCode);
                return Ok(_mapper.Map<OperationBreakdownReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "operation-breakdown-report-view")]
        public async Task<IActionResult> GetPdfAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            try
            {
                var report = await _operationBreakdownReportService.GetReportAsync(buyerCode, order, typeCode, styleCode);
                var pdfBytes = OperationBreakdownReportEngine.GeneratePdf(report);
                var safeFileName = $"OperationBreakdown_{buyerCode}_{order}_{styleCode}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
