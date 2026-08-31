using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.GeneralInventory
{
    [Route("api/general-grn-listing-reports")]
    [ApiController]
    [Authorize(Policy = "general-grn-listing-report")]
    public class GeneralGrnListingReportController : ControllerBase
    {
        private readonly IGeneralGrnListingReportService _generalGrnListingReportService;
        private readonly IMapper _mapper;

        public GeneralGrnListingReportController(IGeneralGrnListingReportService generalGrnListingReportService, IMapper mapper)
        {
            _generalGrnListingReportService = generalGrnListingReportService;
            _mapper = mapper;
        }

        // GET: api/general-grn-listing-reports/header?fromDate=1994-03-01&toDate=1994-06-30&storeCode=M-S&supplierCode=41
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? storeCode, [FromQuery] string? supplierCode)
        {
            try
            {
                var header = await _generalGrnListingReportService.GetHeaderAsync(fromDate, toDate, storeCode, supplierCode);
                return Ok(_mapper.Map<GeneralGrnListingReportHeaderAPIModel>(header));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/general-grn-listing-reports/lines?fromDate=1994-03-01&toDate=1994-06-30&storeCode=M-S&supplierCode=41
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? storeCode, [FromQuery] string? supplierCode)
        {
            try
            {
                var lines = await _generalGrnListingReportService.GetLinesAsync(fromDate, toDate, storeCode, supplierCode);
                return Ok(_mapper.Map<List<GeneralGrnListingReportLineAPIModel>>(lines));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/general-grn-listing-reports/pdf?fromDate=1994-03-01&toDate=1994-06-30&storeCode=M-S&supplierCode=41
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] string? storeCode, [FromQuery] string? supplierCode)
        {
            try
            {
                var header = await _generalGrnListingReportService.GetHeaderAsync(fromDate, toDate, storeCode, supplierCode);
                var lines = await _generalGrnListingReportService.GetLinesAsync(fromDate, toDate, storeCode, supplierCode);

                byte[] pdfBytes = GeneralGrnListingReportEngine.GenerateGrnListingReportPdf(header, lines);
                string safeFileName = $"GeneralGrnListing_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
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
