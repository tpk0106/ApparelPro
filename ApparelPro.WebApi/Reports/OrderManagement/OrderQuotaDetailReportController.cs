using apparelPro.BusinessLogic.Reports.OrderManagement.OrderQuotaDetailReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_ROQ1.PRG's "ORDER QUOTA REPORT" - Reports -> Order Management ->
    // Order/Quota Detail in this app's nav.
    [Route("api/order-quota-detail-report")]
    [ApiController]
    public class OrderQuotaDetailReportController : ControllerBase
    {
        private readonly IOrderQuotaDetailReportService _orderQuotaDetailReportService;
        private readonly IMapper _mapper;

        public OrderQuotaDetailReportController(IOrderQuotaDetailReportService orderQuotaDetailReportService, IMapper mapper)
        {
            _orderQuotaDetailReportService = orderQuotaDetailReportService;
            _mapper = mapper;
        }

        // GET: api/order-quota-detail-report/details?buyerCode=&order=
        [HttpGet("details")]
        [Authorize(Policy = "order-quota-detail-report")]
        public async Task<IActionResult> GetDetails([FromQuery] int? buyerCode, [FromQuery] string? order)
        {
            try
            {
                var report = await _orderQuotaDetailReportService.GetOrderQuotaDetailReportAsync(buyerCode, order);
                var apiModel = _mapper.Map<OrderQuotaDetailReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Order Quota Detail Report: {ex.Message}" });
            }
        }

        // GET: api/order-quota-detail-report/pdf?buyerCode=&order=
        [HttpGet("pdf")]
        [Authorize(Policy = "order-quota-detail-report")]
        public async Task<IActionResult> GetPdf([FromQuery] int? buyerCode, [FromQuery] string? order)
        {
            try
            {
                var report = await _orderQuotaDetailReportService.GetOrderQuotaDetailReportAsync(buyerCode, order);
                byte[] pdfBytes = OrderQuotaDetailReportEngine.GeneratePdf(report);
                string safeFileName = $"OrderQuotaDetailReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Order Quota Detail Report PDF document: {ex.Message}" });
            }
        }
    }
}
