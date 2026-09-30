using apparelPro.BusinessLogic.Reports.OrderManagement.TrimSheet;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // 🎓 Replicates OD_TRIM.PRG's "TRIM SHEET" report - Order Management -> Material Consumption
    // -> Trim Sheet in legacy, exposed here under Reports -> Order Management -> Trim Sheet per
    // this project's current nav structure. See TrimSheetReportServiceModel's SCOPE NOTE for
    // what's intentionally not covered yet (Sub Contract / Production Line costs).
    [Route("api/trim-sheet-report")]
    [ApiController]
    public class TrimSheetReportController : ControllerBase
    {
        private readonly ITrimSheetReportService _trimSheetReportService;
        private readonly IMapper _mapper;

        // 🎓 IAuthorizationService lets us evaluate the "trim-sheet-profit" permission
        // DYNAMICALLY at runtime — it goes through DynamicPermissionPolicyProvider →
        // PermissionAuthorizationHandler → RolePermissionCache → RolePermissions table.
        // This means admins can grant/revoke profit visibility per role from the
        // Permission Matrix UI without any code change or redeployment.
        // Replaces the old hardcoded ProfitVisibilityRoles string[] + User.IsInRole() check.
        private readonly IAuthorizationService _authorizationService;

        public TrimSheetReportController(
            ITrimSheetReportService trimSheetReportService,
            IMapper mapper,
            IAuthorizationService authorizationService)
        {
            _trimSheetReportService = trimSheetReportService;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }

        // 🎓 Checks the "trim-sheet-profit" permission via the dynamic permission system.
        // The permission key matches the DefaultCatalog entry in PermissionService.cs,
        // which mirrors legacy's separate access('trimprof') gate on top of general
        // Trim Sheet viewing. Returns true if the caller's role has been granted this
        // permission in the RolePermissions table (managed via the Permission Matrix UI).
        private async Task<bool> CallerCanSeeProfitAsync()
        {
            var result = await _authorizationService.AuthorizeAsync(User, "trim-sheet-profit");
            return result.Succeeded;
        }

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
                var canSeeProfit = await CallerCanSeeProfitAsync();
                var report = await _trimSheetReportService.GetTrimSheetReportAsync(buyerCode, order, typeCode, styleCode, canSeeProfit);
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
                var canSeeProfit = await CallerCanSeeProfitAsync();
                var report = await _trimSheetReportService.GetTrimSheetReportAsync(buyerCode, order, typeCode, styleCode, canSeeProfit);
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
