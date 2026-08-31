using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.GeneralInventory
{
    [Route("api/general-stock-reorder-reports")]
    [ApiController]
    [Authorize(Policy = "general-stock-reorder-report")]
    public class GeneralStockReorderReportController : ControllerBase
    {
        private readonly IGeneralStockReorderReportService _generalStockReorderReportService;
        private readonly IMapper _mapper;

        public GeneralStockReorderReportController(IGeneralStockReorderReportService generalStockReorderReportService, IMapper mapper)
        {
            _generalStockReorderReportService = generalStockReorderReportService;
            _mapper = mapper;
        }

        // GET: api/general-stock-reorder-reports/header?storeCode=M-S
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] string storeCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");

            try
            {
                var header = await _generalStockReorderReportService.GetHeaderAsync(storeCode);
                return Ok(_mapper.Map<GeneralStockReorderReportHeaderAPIModel>(header));
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

        // GET: api/general-stock-reorder-reports/lines?storeCode=M-S
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] string storeCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");

            try
            {
                var lines = await _generalStockReorderReportService.GetLinesAsync(storeCode);
                return Ok(_mapper.Map<List<GeneralStockReorderReportLineAPIModel>>(lines));
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

        // GET: api/general-stock-reorder-reports/pdf?storeCode=M-S
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] string storeCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
                return BadRequest("Parameter 'storeCode' is required.");

            try
            {
                var header = await _generalStockReorderReportService.GetHeaderAsync(storeCode);
                var lines = await _generalStockReorderReportService.GetLinesAsync(storeCode);

                byte[] pdfBytes = GeneralStockReorderReportEngine.GenerateStockReorderReportPdf(header, lines);
                string safeFileName = $"GeneralStockReorderReport_{storeCode.Trim()}.pdf";
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
                return StatusCode(500, new { Error = $"Failed to construct Stock Re-order Report PDF: {ex.Message}" });
            }
        }
    }
}
