using apparelPro.BusinessLogic.Reports.OrderManagement.GarmentAdditionalCostReport;
using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IGarmentAdditionalCostService;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    // Replicates OD_AITM1.PRG (Update) / OD_AITM2.PRG (Print) - "Additional Costs per
    // Garment" - Order Management -> Material Consumption -> Additional Costs per
    // Garment in legacy. Stock-fabrication side effect deliberately dropped per user
    // decision (2026-08-08), matching MaterialConsumptionService's own prior fix for the
    // same anti-pattern.
    [Route("api/garment-additional-cost")]
    [ApiController]
    public class GarmentAdditionalCostController : ControllerBase
    {
        private readonly IGarmentAdditionalCostService _garmentAdditionalCostService;
        private readonly IMapper _mapper;

        public GarmentAdditionalCostController(IGarmentAdditionalCostService garmentAdditionalCostService, IMapper mapper)
        {
            _garmentAdditionalCostService = garmentAdditionalCostService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "garment-additional-cost-view")]
        [ProducesResponseType(typeof(List<GarmentAdditionalCostAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetGarmentAdditionalCostsAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            if (string.IsNullOrWhiteSpace(order) || string.IsNullOrWhiteSpace(styleCode))
                return BadRequest("Buyer, Order, Type, and Style are all required.");

            var result = await _garmentAdditionalCostService.GetGarmentAdditionalCostsAsync(buyerCode, order, typeCode, styleCode);
            return Ok(_mapper.Map<List<GarmentAdditionalCostAPIModel>>(result));
        }

        [HttpPost]
        [Authorize(Policy = "garment-additional-cost-manage")]
        [ProducesResponseType(typeof(GarmentAdditionalCostAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveGarmentAdditionalCostAsync([FromBody] SaveGarmentAdditionalCostAPIModel request)
        {
            if (request == null) return BadRequest("Entry request data payload cannot be empty.");

            try
            {
                var serviceModel = _mapper.Map<SaveGarmentAdditionalCostServiceModel>(request);
                var result = await _garmentAdditionalCostService.SaveGarmentAdditionalCostAsync(serviceModel);
                return Ok(_mapper.Map<GarmentAdditionalCostAPIModel>(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to save Additional Cost per Garment entry: {ex.Message}" });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "garment-additional-cost-manage")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteGarmentAdditionalCostAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode,
            [FromQuery] string additionalCostCode, [FromQuery] string itemCode)
        {
            try
            {
                var success = await _garmentAdditionalCostService.DeleteGarmentAdditionalCostAsync(
                    buyerCode, order, typeCode, styleCode, additionalCostCode, itemCode);

                if (!success)
                    return BadRequest(new { Message = "Purchase Order already raised against this item. Deletion is blocked to preserve data integrity." });

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Deletion Failed: {ex.Message}" });
            }
        }

        [HttpGet("report/details")]
        [Authorize(Policy = "garment-additional-cost-view")]
        [ProducesResponseType(typeof(GarmentAdditionalCostReportAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetGarmentAdditionalCostReportAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            if (string.IsNullOrWhiteSpace(order) || string.IsNullOrWhiteSpace(styleCode))
                return BadRequest("Buyer, Order, Type, and Style are all required.");

            try
            {
                var report = await _garmentAdditionalCostService.GetGarmentAdditionalCostReportAsync(buyerCode, order, typeCode, styleCode);
                return Ok(_mapper.Map<GarmentAdditionalCostReportAPIModel>(report));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Additional Cost per Garment report: {ex.Message}" });
            }
        }

        [HttpGet("report/pdf")]
        [Authorize(Policy = "garment-additional-cost-view")]
        public async Task<IActionResult> GetGarmentAdditionalCostReportPdfAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            if (string.IsNullOrWhiteSpace(order) || string.IsNullOrWhiteSpace(styleCode))
                return BadRequest("Buyer, Order, Type, and Style are all required.");

            try
            {
                var report = await _garmentAdditionalCostService.GetGarmentAdditionalCostReportAsync(buyerCode, order, typeCode, styleCode);
                byte[] pdfBytes = GarmentAdditionalCostReportEngine.GeneratePdf(report);
                string safeFileName = $"AdditionalCostsPerGarment_{styleCode.Trim()}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Additional Costs per Garment PDF document: {ex.Message}" });
            }
        }
    }
}
