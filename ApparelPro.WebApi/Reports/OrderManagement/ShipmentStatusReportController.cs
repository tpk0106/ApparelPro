using apparelPro.BusinessLogic.Reports.OrderManagement.ShipmentStatusReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_SHPST.PRG's "SHIPMENT STATUS REPORT" - Reports -> Order Management ->
    // Shipment status Report in this app's nav.
    [Route("api/shipment-status-report")]
    [ApiController]
    public class ShipmentStatusReportController : ControllerBase
    {
        private readonly IShipmentStatusReportService _shipmentStatusReportService;
        private readonly IMapper _mapper;

        public ShipmentStatusReportController(IShipmentStatusReportService shipmentStatusReportService, IMapper mapper)
        {
            _shipmentStatusReportService = shipmentStatusReportService;
            _mapper = mapper;
        }

        // GET: api/shipment-status-report/details?buyerCode=&order=
        [HttpGet("details")]
        [Authorize(Policy = "shipment-status-report")]
        public async Task<IActionResult> GetDetails([FromQuery] int buyerCode, [FromQuery] string order)
        {
            try
            {
                var report = await _shipmentStatusReportService.GetShipmentStatusReportAsync(buyerCode, order);
                var apiModel = _mapper.Map<ShipmentStatusReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Shipment Status Report: {ex.Message}" });
            }
        }

        // GET: api/shipment-status-report/pdf?buyerCode=&order=
        [HttpGet("pdf")]
        [Authorize(Policy = "shipment-status-report")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order)
        {
            try
            {
                var report = await _shipmentStatusReportService.GetShipmentStatusReportAsync(buyerCode, order);
                byte[] pdfBytes = ShipmentStatusReportEngine.GeneratePdf(report);
                string safeFileName = $"ShipmentStatusReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Shipment Status Report PDF document: {ex.Message}" });
            }
        }
    }
}
