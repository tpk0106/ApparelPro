using apparelPro.BusinessLogic.Reports.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Reports -> G. Manpower Requirement - migrated from PR_REP3.PRG.
    [Route("api/manpower-requirement-report")]
    [ApiController]
    public class ManpowerRequirementReportController : ControllerBase
    {
        private readonly IManpowerRequirementReportService _manpowerRequirementReportService;
        private readonly IMapper _mapper;

        public ManpowerRequirementReportController(
            IManpowerRequirementReportService manpowerRequirementReportService, IMapper mapper)
        {
            _manpowerRequirementReportService = manpowerRequirementReportService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "manpower-requirement-report-view")]
        [ProducesResponseType(typeof(ManpowerRequirementReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode,
            [FromQuery] string styleCode, [FromQuery] string? lineCode)
        {
            try
            {
                var report = await _manpowerRequirementReportService.GetReportAsync(buyerCode, order, typeCode, styleCode, lineCode);
                return Ok(_mapper.Map<ManpowerRequirementReportAPIModel>(report));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpGet("pdf")]
        [Authorize(Policy = "manpower-requirement-report-view")]
        public async Task<IActionResult> GetPdfAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode,
            [FromQuery] string styleCode, [FromQuery] string? lineCode)
        {
            try
            {
                var report = await _manpowerRequirementReportService.GetReportAsync(buyerCode, order, typeCode, styleCode, lineCode);
                var pdfBytes = ManpowerRequirementReportEngine.GeneratePdf(report);
                var safeFileName = $"ManpowerRequirement_{buyerCode}_{order}_{styleCode}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
