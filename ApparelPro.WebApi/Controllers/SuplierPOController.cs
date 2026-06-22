using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/supplier-po")]
    [ApiController]
    public class SuplierPOController : ControllerBase
    {
        private readonly ISupplierPurchaseOrderService _supplierPurchaseOrderService;

        public SuplierPOController(ISupplierPurchaseOrderService supplierPurchaseOrderService)
        {
            _supplierPurchaseOrderService = supplierPurchaseOrderService;
        }

        // 1. GET: api/purchaseOrder/unfulfilled-budget?buyerCode=1&order=1068
        [HttpGet("unfulfilled-budget")]
        public async Task<IActionResult> GetUnfulfilledBudget([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrEmpty(order)) return BadRequest("Target order parameter cannot be empty.");
            try
            {
                var lines = await _supplierPurchaseOrderService.GetUnfulfilledBudgetLinesAsync(buyerCode, order);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to look up unfulfilled material thresholds: {ex.Message}" });
            }
        }

        // 2. POST: api/purchaseOrder/save-supplier-po
        [HttpPost("save-supplier-po")]
        public async Task<IActionResult> SaveSupplierPO([FromBody] SaveSupplierPORequestServiceModel request)
        {
            if (request == null || request.Header == null) return BadRequest("Purchase order payload cannot be empty.");
            try
            {
                var success = await _supplierPurchaseOrderService.SaveSupplierPurchaseOrderAsync(request);
                return Ok(success);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Procurement Save Transaction Failed: {ex.Message}" });
            }
        }
    }
}
