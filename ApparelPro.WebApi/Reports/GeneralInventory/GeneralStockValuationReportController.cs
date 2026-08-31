using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.GeneralInventory
{
    [Route("api/general-stock-valuation-reports")]
    [ApiController]
    [Authorize(Policy = "general-stock-valuation-report")]
    public class GeneralStockValuationReportController : ControllerBase
    {
        private readonly IGeneralStockValuationReportService _generalStockValuationReportService;
        private readonly IMapper _mapper;

        public GeneralStockValuationReportController(IGeneralStockValuationReportService generalStockValuationReportService, IMapper mapper)
        {
            _generalStockValuationReportService = generalStockValuationReportService;
            _mapper = mapper;
        }

        // GET: api/general-stock-valuation-reports/header?storeCode=M-S&fromItemCode=...&toItemCode=...
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] string storeCode, [FromQuery] string fromItemCode, [FromQuery] string toItemCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (string.IsNullOrWhiteSpace(fromItemCode))
                return BadRequest("Parameter 'fromItemCode' is required.");
            if (string.IsNullOrWhiteSpace(toItemCode))
                return BadRequest("Parameter 'toItemCode' is required.");

            try
            {
                var header = await _generalStockValuationReportService.GetHeaderAsync(storeCode, fromItemCode, toItemCode);
                return Ok(_mapper.Map<GeneralStockValuationReportHeaderAPIModel>(header));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/general-stock-valuation-reports/lines?storeCode=M-S&fromItemCode=...&toItemCode=...
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] string storeCode, [FromQuery] string fromItemCode, [FromQuery] string toItemCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (string.IsNullOrWhiteSpace(fromItemCode))
                return BadRequest("Parameter 'fromItemCode' is required.");
            if (string.IsNullOrWhiteSpace(toItemCode))
                return BadRequest("Parameter 'toItemCode' is required.");

            try
            {
                var lines = await _generalStockValuationReportService.GetLinesAsync(storeCode, fromItemCode, toItemCode);
                return Ok(_mapper.Map<List<GeneralStockValuationReportLineAPIModel>>(lines));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/general-stock-valuation-reports/pdf?storeCode=M-S&fromItemCode=...&toItemCode=...
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] string storeCode, [FromQuery] string fromItemCode, [FromQuery] string toItemCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");
            if (string.IsNullOrWhiteSpace(fromItemCode))
                return BadRequest("Parameter 'fromItemCode' is required.");
            if (string.IsNullOrWhiteSpace(toItemCode))
                return BadRequest("Parameter 'toItemCode' is required.");

            try
            {
                var header = await _generalStockValuationReportService.GetHeaderAsync(storeCode, fromItemCode, toItemCode);
                var lines = await _generalStockValuationReportService.GetLinesAsync(storeCode, fromItemCode, toItemCode);

                byte[] pdfBytes = GeneralStockValuationReportEngine.GenerateStockValuationReportPdf(header, lines);
                string safeFileName = $"GeneralStockValuationReport_{storeCode.Trim()}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Stock Valuation Report PDF: {ex.Message}" });
            }
        }
    }
}
