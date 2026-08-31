using apparelPro.BusinessLogic.Reports.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderwiseInventory
{
    [Route("api/raw-material-control-sheet-reports")]
    [ApiController]
    [Authorize(Policy = "raw-material-control-sheet-report")]
    public class RawMaterialControlSheetController : ControllerBase
    {
        private readonly IRawMaterialControlSheetService _rawMaterialControlSheetService;
        private readonly IMapper _mapper;

        public RawMaterialControlSheetController(IRawMaterialControlSheetService rawMaterialControlSheetService, IMapper mapper)
        {
            _rawMaterialControlSheetService = rawMaterialControlSheetService;
            _mapper = mapper;
        }

        // GET: api/raw-material-control-sheet-reports/header?buyerCode=2&order=1017-18
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var header = await _rawMaterialControlSheetService.GetHeaderAsync(buyerCode, order);
                return Ok(_mapper.Map<RawMaterialControlSheetHeaderAPIModel>(header));
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

        // GET: api/raw-material-control-sheet-reports/lines?buyerCode=2&order=1017-18
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var lines = await _rawMaterialControlSheetService.GetLinesAsync(buyerCode, order);
                return Ok(_mapper.Map<List<RawMaterialControlSheetLineAPIModel>>(lines));
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

        // GET: api/raw-material-control-sheet-reports/pdf?buyerCode=2&order=1017-18
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var header = await _rawMaterialControlSheetService.GetHeaderAsync(buyerCode, order);
                var lines = await _rawMaterialControlSheetService.GetLinesAsync(buyerCode, order);

                byte[] pdfBytes = RawMaterialControlSheetEngine.GenerateRawMaterialControlSheetPdf(header, lines);
                string safeFileName = $"RawMaterialControlSheet_{buyerCode}_{order}.pdf";
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
                return StatusCode(500, new { Error = $"Failed to construct Raw Material Control Sheet PDF: {ex.Message}" });
            }
        }
    }
}
