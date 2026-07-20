using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/material-consumption")]
    [ApiController]
    [Authorize(Roles = "Merchandiser,Merchandiser Manager")]
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
                    bool isHigherAuthority = User.IsInRole("Merchandising Manager") || User.IsInRole("Merchandiser Manager") || User.IsInRole("Executive Director");

                    if (!isHigherAuthority)
                    {
                        // FIXED TYPING & FORMATTING PASS: 
                        // Safely calls ToString() on the DateOnly struct using standard day-month-year masks
                        string formattedDate = approvalDetails.EstimateApprovalDate.HasValue
                            ? approvalDetails.EstimateApprovalDate.Value.ToString("dd-MMM-yyyy")
                            : "an Unknown Date";

                        return StatusCode(403, new
                        {
                            Error = $"🛑 ACCESS DENIED: This material sheet was officially approved and locked by [ {approvalDetails.EstimateApprovalUserName} ] on {formattedDate}. Alterations are restricted to higher management authority accounts only."
                        });
                    }
                }

                var createMaterialConsumptionEntryRequestServiceModel = _mapper.Map<CreateMaterialConsumptionEntryRequestServiceModel>(request);
                var result = await _materialConsumptionService.SaveMaterialConsumptionEntryAsync(createMaterialConsumptionEntryRequestServiceModel);
                return Ok(result);
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
            [FromQuery] string stockCode, [FromQuery] string itemCode, [FromQuery] string color, [FromQuery] string size)
        {
            try
            {
                var success = await _materialConsumptionService.DeleteConsumptionEntryAsync(
                    buyerCode, order, typeCode, styleCode, stockCode, itemCode, color, size
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

        //[HttpGet("style-dimensions")]
        //[ProducesResponseType(typeof(StyleDimensionsLookupAPIModel), HttpStatusCodes.OK)]
        //public async Task<IActionResult> GetStyleDimensions(
        //    [FromQuery] int buyerCode,
        //    [FromQuery] string order,
        //    [FromQuery] int typeCode,
        //    [FromQuery] string styleCode)
        //{
        //    try
        //    {
        //        var data = await _materialConsumptionService.GetStyleDimensionsAsync(buyerCode, order, typeCode, styleCode);
        //       var styleDimensionsLookupAPIModel = _mapper.Map<StyleDimensionsLookupAPIModel>(data);
        //        return Ok(styleDimensionsLookupAPIModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Error = $"Failed to look up style colour/size dimensional metrics: {ex.Message}" });
        //    }
        //}

    }
}
