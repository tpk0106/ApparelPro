using apparelPro.BusinessLogic.Reports.OrderManagement.PurchaseOrderListReport;
using apparelPro.BusinessLogic.Services.interfaces.Reports.OrderManagement;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_POLST.PRG's "PURCHASE ORDER LIST" report - Reports -> Order
    // Management -> List of P/O's in this app's nav. See
    // PurchaseOrderListReportServiceModel's SCOPE NOTE for the P/O Date/Time gap.
    [Route("api/purchase-order-list-report")]
    [ApiController]
    public class PurchaseOrderListReportController : ControllerBase
    {
        private readonly IPurchaseOrderListReportService _purchaseOrderListReportService;
        private readonly IMapper _mapper;

        public PurchaseOrderListReportController(IPurchaseOrderListReportService purchaseOrderListReportService, IMapper mapper)
        {
            _purchaseOrderListReportService = purchaseOrderListReportService;
            _mapper = mapper;
        }

        // GET: api/purchase-order-list-report/po-numbers
        [HttpGet("po-numbers")]
        [Authorize(Policy = "purchase-order-list-report")]
        public async Task<IActionResult> GetPurchaseOrderNumbers()
        {
            var numbers = await _purchaseOrderListReportService.GetPurchaseOrderNumbersAsync();
            return Ok(numbers);
        }

        // GET: api/purchase-order-list-report/details?purchaseOrderNumber=
        [HttpGet("details")]
        [Authorize(Policy = "purchase-order-list-report")]
        public async Task<IActionResult> GetDetails([FromQuery] string purchaseOrderNumber)
        {
            if (string.IsNullOrWhiteSpace(purchaseOrderNumber))
                return BadRequest("Purchase Order No. is required.");

            try
            {
                var report = await _purchaseOrderListReportService.GetPurchaseOrderListReportAsync(purchaseOrderNumber);
                var apiModel = _mapper.Map<PurchaseOrderListReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Purchase Order List Report: {ex.Message}" });
            }
        }

        // GET: api/purchase-order-list-report/pdf?purchaseOrderNumber=
        [HttpGet("pdf")]
        [Authorize(Policy = "purchase-order-list-report")]
        public async Task<IActionResult> GetPdf([FromQuery] string purchaseOrderNumber)
        {
            if (string.IsNullOrWhiteSpace(purchaseOrderNumber))
                return BadRequest("Purchase Order No. is required.");

            try
            {
                var report = await _purchaseOrderListReportService.GetPurchaseOrderListReportAsync(purchaseOrderNumber);
                byte[] pdfBytes = PurchaseOrderListReportEngine.GeneratePdf(report);
                string safeFileName = $"PurchaseOrderList_{purchaseOrderNumber.Trim()}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Purchase Order List Report PDF document: {ex.Message}" });
            }
        }
    }
}
