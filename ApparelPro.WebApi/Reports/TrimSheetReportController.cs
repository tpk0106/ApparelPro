using apparelPro.BusinessLogic.Reports.OrderManagement.TrimSheet;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports
{
    // Replicates OD_TRIM.PRG's "TRIM SHEET" report - Order Management -> Material Consumption
    // -> Trim Sheet in legacy, exposed here under Reports -> Order Management -> Trim Sheet per
    // this project's current nav structure. See TrimSheetReportServiceModel's SCOPE NOTE for
    // what's intentionally not covered yet (Sub Contract / Production Line costs).
    [Route("api/trim-sheet-report")]
    [ApiController]
    public class TrimSheetReportController : ControllerBase
    {
        // Estimated Profit visibility - checked inline here rather than as a second
        // [Authorize] policy, matching MaterialConsumptionController's own higher-authority
        // check style. Kept in sync with the comment on PermissionService's role catalog entry.
        private static readonly string[] ProfitVisibilityRoles = { "Merchandiser Manager", "Merchandising Manager", "Executive Director" };

        private readonly ITrimSheetReportService _trimSheetReportService;
        private readonly IMapper _mapper;

        public TrimSheetReportController(ITrimSheetReportService trimSheetReportService, IMapper mapper)
        {
            _trimSheetReportService = trimSheetReportService;
            _mapper = mapper;
        }

        private bool CallerCanSeeProfit() => ProfitVisibilityRoles.Any(User.IsInRole);

        // GET: api/trim-sheet-report/details?buyerCode=&order=&typeCode=&styleCode=
        [HttpGet("details")]
        [Authorize(Policy = "trim-sheet-report")]
        public async Task<IActionResult> GetDetails(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            if (string.IsNullOrWhiteSpace(order) || string.IsNullOrWhiteSpace(styleCode))
                return BadRequest("Buyer, Order, Type, and Style are all required.");

            try
            {
                var report = await _trimSheetReportService.GetTrimSheetReportAsync(buyerCode, order, typeCode, styleCode, CallerCanSeeProfit());
                var apiModel = _mapper.Map<TrimSheetReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                // Covers both "style/order not found" and the currency-conversion "no rate on
                // file" failure - both are the caller's data problem to fix, not a server fault.
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Trim Sheet report: {ex.Message}" });
            }
        }

        // GET: api/trim-sheet-report/pdf?buyerCode=&order=&typeCode=&styleCode=
        [HttpGet("pdf")]
        [Authorize(Policy = "trim-sheet-report")]
        public async Task<IActionResult> GetPdf(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            if (string.IsNullOrWhiteSpace(order) || string.IsNullOrWhiteSpace(styleCode))
                return BadRequest("Buyer, Order, Type, and Style are all required.");

            try
            {
                var report = await _trimSheetReportService.GetTrimSheetReportAsync(buyerCode, order, typeCode, styleCode, CallerCanSeeProfit());
                byte[] pdfBytes = TrimSheetReportEngine.GeneratePdf(report);
                string safeFileName = $"TrimSheet_{styleCode.Trim()}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Trim Sheet PDF document: {ex.Message}" });
            }
        }
    }
}
