using apparelPro.BusinessLogic.Reports.OrderManagement.OrderDetail;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_RPO1.PRG's "ORDER CONFIRMATION REPORT" - Reports -> Order Management ->
    // Order Detail in this app's nav (the legacy menu item was "B. Order Confirmation
    // Report"). See OrderDetailReportServiceModel's SCOPE NOTE for exactly which legacy
    // case this covers (Buyer+Order given, Type blank - per explicit project decision,
    // 2026-08-07) and the two gaps found and deliberately not guessed.
    [Route("api/order-detail-report")]
    [ApiController]
    public class OrderDetailReportController : ControllerBase
    {
        private readonly IOrderDetailReportService _orderDetailReportService;
        private readonly IMapper _mapper;

        public OrderDetailReportController(IOrderDetailReportService orderDetailReportService, IMapper mapper)
        {
            _orderDetailReportService = orderDetailReportService;
            _mapper = mapper;
        }

        // GET: api/order-detail-report/details?buyerCode=&order=
        [HttpGet("details")]
        [Authorize(Policy = "order-detail-report")]
        public async Task<IActionResult> GetDetails(
            [FromQuery] int buyerCode,
            [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Buyer and Order are both required.");

            try
            {
                var report = await _orderDetailReportService.GetOrderDetailReportAsync(buyerCode, order);
                var apiModel = _mapper.Map<OrderDetailReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                // "Buyer/Order not found" - the caller's data problem to fix, not a server fault.
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Order Detail report: {ex.Message}" });
            }
        }

        // GET: api/order-detail-report/pdf?buyerCode=&order=
        [HttpGet("pdf")]
        [Authorize(Policy = "order-detail-report")]
        public async Task<IActionResult> GetPdf(
            [FromQuery] int buyerCode,
            [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Buyer and Order are both required.");

            try
            {
                var report = await _orderDetailReportService.GetOrderDetailReportAsync(buyerCode, order);
                byte[] pdfBytes = OrderDetailReportEngine.GeneratePdf(report);
                string safeFileName = $"OrderDetail_{order.Trim()}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Order Detail PDF document: {ex.Message}" });
            }
        }
    }
}
