using apparelPro.BusinessLogic.Reports.OrderManagement.ColorSizeReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports
{
    // Replicates OD_CLSZ3.PRG's "COLOUR / SIZE DETAILS" report - Reports -> Order
    // Management -> Colour/Size in this app's nav. See ColorSizeReportServiceModel's
    // SCOPE NOTE for how legacy's od_clqr query is derived from ColorSizeDetails.
    [Route("api/color-size-report")]
    [ApiController]
    public class ColorSizeReportController : ControllerBase
    {
        private readonly IColorSizeReportService _colorSizeReportService;
        private readonly IMapper _mapper;

        public ColorSizeReportController(IColorSizeReportService colorSizeReportService, IMapper mapper)
        {
            _colorSizeReportService = colorSizeReportService;
            _mapper = mapper;
        }

        // GET: api/color-size-report/details?buyerCode=&order=
        [HttpGet("details")]
        [Authorize(Policy = "color-size-report")]
        public async Task<IActionResult> GetDetails([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Buyer and Order are both required.");

            try
            {
                var report = await _colorSizeReportService.GetColorSizeReportAsync(buyerCode, order);
                var apiModel = _mapper.Map<ColorSizeReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Colour/Size Report: {ex.Message}" });
            }
        }

        // GET: api/color-size-report/pdf?buyerCode=&order=
        [HttpGet("pdf")]
        [Authorize(Policy = "color-size-report")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Buyer and Order are both required.");

            try
            {
                var report = await _colorSizeReportService.GetColorSizeReportAsync(buyerCode, order);
                byte[] pdfBytes = ColorSizeReportEngine.GeneratePdf(report);
                string safeFileName = $"ColorSizeReport_{order.Trim()}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Colour/Size Report PDF document: {ex.Message}" });
            }
        }
    }
}
