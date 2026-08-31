using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/orderwise-grn-listing-reports")]
    [ApiController]
    [Authorize(Policy = "orderwise-grn-listing-report")]
    public class GrnListingReportController : ControllerBase
    {
        private readonly IGrnListingReportService _grnListingReportService;
        private readonly IMapper _mapper;

        public GrnListingReportController(IGrnListingReportService grnListingReportService, IMapper mapper)
        {
            _grnListingReportService = grnListingReportService;
            _mapper = mapper;
        }

        // GET: api/orderwise-grn-listing-reports/header?fromDate=1994-03-01&toDate=1994-06-30&buyerCode=2&order=1017-18&storeCode=M-S&supplierCode=41
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader(
            [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, [FromQuery] int? buyerCode,
            [FromQuery] string? order, [FromQuery] string? storeCode, [FromQuery] string? supplierCode)
        {
            try
            {
                var header = await _grnListingReportService.GetHeaderAsync(fromDate, toDate, buyerCode, order, storeCode, supplierCode);
                return Ok(_mapper.Map<GrnListingReportHeaderAPIModel>(header));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/orderwise-grn-listing-reports/lines?fromDate=1994-03-01&toDate=1994-06-30&buyerCode=2&order=1017-18&storeCode=M-S&supplierCode=41
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines(
            [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, [FromQuery] int? buyerCode,
            [FromQuery] string? order, [FromQuery] string? storeCode, [FromQuery] string? supplierCode)
        {
            try
            {
                var lines = await _grnListingReportService.GetLinesAsync(fromDate, toDate, buyerCode, order, storeCode, supplierCode);
                return Ok(_mapper.Map<List<GrnListingReportLineAPIModel>>(lines));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/orderwise-grn-listing-reports/pdf?fromDate=1994-03-01&toDate=1994-06-30&buyerCode=2&order=1017-18&storeCode=M-S&supplierCode=41
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf(
            [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, [FromQuery] int? buyerCode,
            [FromQuery] string? order, [FromQuery] string? storeCode, [FromQuery] string? supplierCode)
        {
            try
            {
                var header = await _grnListingReportService.GetHeaderAsync(fromDate, toDate, buyerCode, order, storeCode, supplierCode);
                var lines = await _grnListingReportService.GetLinesAsync(fromDate, toDate, buyerCode, order, storeCode, supplierCode);

                byte[] pdfBytes = GrnListingReportEngine.GenerateGrnListingReportPdf(header, lines);
                string safeFileName = buyerCode.HasValue ? $"GrnListing_{buyerCode}_{order}.pdf" : $"GrnListing_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct GRN Listing PDF: {ex.Message}" });
            }
        }
    }
}
