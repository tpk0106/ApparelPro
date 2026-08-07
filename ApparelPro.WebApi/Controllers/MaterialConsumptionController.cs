using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using ApparelPro.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/material-consumption")]
    [ApiController]
    [Authorize(Policy = "material-consumption")]
    public class MaterialConsumptionController : ControllerBase
    {
        private readonly IMaterialConsumptionService _materialConsumptionService;
        private readonly IStyleApprovalService _styleApprovalService;
        private readonly IMapper _mapper;

        public MaterialConsumptionController(IMaterialConsumptionService consumptionService, 
            IMapper mapper, IStyleApprovalService styleApprovalService)
        {
            _materialConsumptionService = consumptionService;
            _styleApprovalService = styleApprovalService;
            _mapper = mapper;
        }       

        [HttpGet("feature-headers")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(OrderItemFeatureAPIModel),HttpStatusCodes.OK)]        
        [ProducesResponseType(typeof(NotFoundResult), HttpStatusCodes.NotFound)]
        [SwaggerOperation(Tags = new[] { "order Item Feature Endpoints" },
             Summary = "Four Item Features for an Order Item in the style",
             Description = "Returns 200 - OK with OrderItemFeatureAPIModel model.")
         ]
        public async Task<IActionResult> GetFeatureHeaders([FromQuery] string stockCode, [FromQuery] string itemCode)
        {
            var data = await _materialConsumptionService.GetDynamicFeatureHeadersAsync(stockCode, itemCode);
            var orderItemFeatureAPIModel = _mapper.Map<OrderItemFeatureAPIModel>(data);
            if (data == null) return NotFound("No dynamic feature mapping rules configured for this item selection.");
            return Ok(orderItemFeatureAPIModel);
        }

        [HttpGet("calculate-consumption")]
        [ProducesResponseType(typeof(decimal), HttpStatusCodes.OK)]
        public async Task<IActionResult> CalculateConsumption(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode,
            [FromQuery] string? garmentColor,
            [FromQuery] string? garmentSize,
            [FromQuery] string parentOrderUnit,
            [FromQuery] string consumptionUnit,
            [FromQuery] string finalItemUnit,
            [FromQuery] decimal quantityPerGarment,
            [FromQuery] decimal allowancePercentage)
        {
            try
            {
                var totalRequiredConsumption = await _materialConsumptionService.CalculateMaterialConsumptionAsync(
                    buyerCode, order, typeCode, styleCode, garmentColor, garmentSize,
                    parentOrderUnit, consumptionUnit, finalItemUnit, quantityPerGarment, allowancePercentage
                );

                return Ok(totalRequiredConsumption);
            }
            catch (InvalidOperationException ex)
            {
                // Catches and returns clear alerts if conversion maps are missing in the DB tables
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transactional Math Error: {ex.Message}" });
            }
        }

        [HttpPost("save-entry")]
        //[Authorize] // Enforces that the request must contain a valid active web session token
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveEntry([FromBody] CreateMaterialConsumptionEntryRequestAPIModel request)
        {
            if (request == null) return BadRequest("Entry request data payload cannot be empty.");

            try
            {

                // 1. ISOLATED SERVICE INVOCATION PASS: Check for active locks in the database
                //var approvalDetails = await _materialConsumptionService.GetStyleApprovalDetailsAsync(
                //    request.BuyerCode, request.Order, request.TypeCode, request.StyleCode
                //);

                // 1. Invoke your existing style approval service layer
                var approvalDetails = await _styleApprovalService.GetStyleApprovalDetailsAsync(
                    request.BuyerCode, request.Order, request.TypeCode, request.StyleCode
                );

                if (approvalDetails != null)
                {
                    // 🚀 THE CLIPPER SECURITY GUARD ACCESS INTERCEPTOR:
                    // FIXED (2026-08-07): this list omitted "Administrator", the same recurring
                    // oversight already fixed twice elsewhere in this project (Trim Sheet Report's
                    // TrimSheetReportRoles, and the still-open StyleApprovalOnlyRoles note) -
                    // every other authority/role gate in this app defaults Administrator in.
                    bool isHigherAuthority = User.IsInRole("Administrator") || User.IsInRole("Merchandising Manager") || User.IsInRole("Merchandiser Manager") || User.IsInRole("Executive Director");

                    if (!isHigherAuthority)
                    {
                        // FIXED TYPING & FORMATTING PASS: 
                        // Safely calls ToString() on the DateOnly struct using standard day-month-year masks
                        string formattedDate = approvalDetails.EstimateApprovalDate.HasValue
                            ? approvalDetails.EstimateApprovalDate.Value.ToString("dd-MMM-yyyy")
                            : "an Unknown Date";

                        return StatusCode(403, new
                        {
                            // Leads with "Style already approved on {date}" per explicit user
                            // request (2026-08-07) - the previous "ACCESS DENIED" paragraph was
                            // being swallowed by two separate frontend bugs (axiosClient not
                            // parsing the `error` field, and this screen's catch block showing a
                            // hardcoded generic message), so once those were fixed the wording
                            // itself also needed to lead with the actionable fact instead of a
                            // dramatic banner.
                            Error = $"Style already approved on {formattedDate} by {approvalDetails.EstimateApprovalUserName}. Only a Merchandising Manager, Merchandiser Manager, or Executive Director can edit it now."
                        });
                    }
                }

                var createMaterialConsumptionEntryRequestServiceModel = _mapper.Map<CreateMaterialConsumptionEntryRequestServiceModel>(request);
                var result = await _materialConsumptionService.SaveMaterialConsumptionEntryAsync(createMaterialConsumptionEntryRequestServiceModel);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Manual/Calculate Consumption validation (2026-08-07) - e.g. "Total Consumption
                // must be greater than zero when entered manually" - is the caller's data problem
                // to fix, not a server fault, so it gets its own clean 400 rather than falling
                // through to the generic 500 below (matching TrimSheetReportController's pattern).
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to execute material ledger save transaction: {ex.Message}" });
            }
        }

        [HttpGet("by-style")]
        [ProducesResponseType(typeof(List<StyleMaterialConsumptionLedgerRowAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetBreakdownByStyle(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(styleCode))
                return BadRequest("Target order and style reference tracking parameters cannot be empty.");

            try
            {
                var ledgerRows = await _materialConsumptionService.GetLedgerEntriesByStyleAsync(buyerCode, order, typeCode, styleCode);
                return Ok(_mapper.Map<List<StyleMaterialConsumptionLedgerRowAPIModel>>(ledgerRows));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to read production spreadsheet matrix records: {ex.Message}" });
            }
        }

        [HttpDelete("delete-entry")]
        //[ProducesResponseType(HttpStatusCodes.NoContent)]
        
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> DeleteEntry(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode,
            // Nullable: with <Nullable>enable</Nullable>, ASP.NET Core implicitly treats a
            // non-nullable string action parameter as [Required], and RequiredAttribute rejects
            // an empty string as well as a missing one. A blank/universal Color or Size is a
            // legitimate value here (matches how it's stored - see StyleMaterialConsumptionLedger),
            // so these must be nullable to avoid a false "field is required" validation error.
            [FromQuery] string stockCode, [FromQuery] string itemCode, [FromQuery] string? color, [FromQuery] string? size)
        {
            try
            {
                var success = await _materialConsumptionService.DeleteConsumptionEntryAsync(
                    buyerCode, order, typeCode, styleCode, stockCode, itemCode, color ?? "", size ?? ""
                );

                if (!success)
                {
                    return BadRequest(new { Message = "Purchase Order already raised. Deletion is blocked to preserve data integrity bounds." });
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Deletion Failed: {ex.Message}" });
            }
        }

        [HttpGet("items-lookup")]
        [ProducesResponseType(typeof(List<OrderItemServiceModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetItemsLookup(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            var data = await _materialConsumptionService.GetAvailableMaterialsLookupAsync(buyerCode, order, typeCode, styleCode);
            return Ok(data);
        }

        [HttpGet("catalog")]
        [ProducesResponseType(typeof(List<MaterialCatalogGroupAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "order Item Feature Endpoints" },
             Summary = "Full material catalog grouped by Stock category",
             Description = "Returns every Stock category with its Item catalog, for the material picker UI. Not scoped to a style.")
         ]
        public async Task<IActionResult> GetMaterialCatalog()
        {
            var data = await _materialConsumptionService.GetMaterialCatalogAsync();
            var result = _mapper.Map<List<MaterialCatalogGroupAPIModel>>(data);
            return Ok(result);
        }     

        [HttpPost("copy-from-style")]
        [ProducesResponseType(typeof(CopyMaterialsFromStyleResultAPIModel), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "order Item Feature Endpoints" },
             Summary = "Bulk-copy all material lines from another Buyer/Order/Type/Style",
             Description = "Copies every StyleMaterialConsumptionLedger/StyleMaterialCostProfiles line from a source style into the target style, skipping any item that already exists at the target.")
         ]
        public async Task<IActionResult> CopyMaterialsFromStyle([FromBody] CopyMaterialsFromStyleRequestAPIModel request)
        {
            if (request == null) return BadRequest("Copy request data payload cannot be empty.");

            try
            {
                var approvalDetails = await _styleApprovalService.GetStyleApprovalDetailsAsync(
                    request.TargetBuyerCode, request.TargetOrder, request.TargetTypeCode, request.TargetStyleCode
                );

                if (approvalDetails != null)
                {
                    // FIXED (2026-08-07): same Administrator omission fix as SaveEntry above.
                    bool isHigherAuthority = User.IsInRole("Administrator") || User.IsInRole("Merchandising Manager") || User.IsInRole("Merchandiser Manager") || User.IsInRole("Executive Director");

                    if (!isHigherAuthority)
                    {
                        string formattedDate = approvalDetails.EstimateApprovalDate.HasValue
                            ? approvalDetails.EstimateApprovalDate.Value.ToString("dd-MMM-yyyy")
                            : "an Unknown Date";

                        return StatusCode(403, new
                        {
                            // Leads with "Style already approved on {date}" per explicit user
                            // request (2026-08-07) - the previous "ACCESS DENIED" paragraph was
                            // being swallowed by two separate frontend bugs (axiosClient not
                            // parsing the `error` field, and this screen's catch block showing a
                            // hardcoded generic message), so once those were fixed the wording
                            // itself also needed to lead with the actionable fact instead of a
                            // dramatic banner.
                            Error = $"Style already approved on {formattedDate} by {approvalDetails.EstimateApprovalUserName}. Only a Merchandising Manager, Merchandiser Manager, or Executive Director can edit it now."
                        });
                    }
                }

                var serviceRequest = _mapper.Map<CopyMaterialsFromStyleRequestServiceModel>(request);
                var result = await _materialConsumptionService.CopyMaterialsFromStyleAsync(serviceRequest);
                return Ok(_mapper.Map<CopyMaterialsFromStyleResultAPIModel>(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to copy materials from source style: {ex.Message}" });
            }
        }

    }
}
