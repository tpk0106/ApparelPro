using apparelPro.BusinessLogic.Reports.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using ApparelPro.WebApi.APIModels.GeneralInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.GeneralInventory
{
    [Route("api/general-purchase-order-list-reports")]
    [ApiController]
    [Authorize(Policy = "general-purchase-order-list-report")]
    public class GeneralPurchaseOrderListReportController : ControllerBase
    {
        private readonly IGeneralPurchaseOrderListReportService _generalPurchaseOrderListReportService;
        private readonly IMapper _mapper;

        public GeneralPurchaseOrderListReportController(IGeneralPurchaseOrderListReportService generalPurchaseOrderListReportService, IMapper mapper)
        {
            _generalPurchaseOrderListReportService = generalPurchaseOrderListReportService;
            _mapper = mapper;
        }

        // GET: api/general-purchase-order-list-reports/header?fromDate=1994-06-01&toDate=1994-06-30
        [HttpGet("header")]
        public async Task<IActionResult> GetHeader([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var header = await _generalPurchaseOrderListReportService.GetHeaderAsync(fromDate, toDate);
                return Ok(_mapper.Map<GeneralPurchaseOrderListReportHeaderAPIModel>(header));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/general-purchase-order-list-reports/lines?fromDate=1994-06-01&toDate=1994-06-30
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var lines = await _generalPurchaseOrderListReportService.GetLinesAsync(fromDate, toDate);
                return Ok(_mapper.Map<List<GeneralPurchaseOrderListReportLineAPIModel>>(lines));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // GET: api/general-purchase-order-list-reports/pdf?fromDate=1994-06-01&toDate=1994-06-30
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            try
            {
                var header = await _generalPurchaseOrderListReportService.GetHeaderAsync(fromDate, toDate);
                var lines = await _generalPurchaseOrderListReportService.GetLinesAsync(fromDate, toDate);

                byte[] pdfBytes = GeneralPurchaseOrderListReportEngine.GeneratePurchaseOrderListReportPdf(header, lines);
                string safeFileName = $"GeneralPurchaseOrderList_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct List of P/O's PDF: {ex.Message}" });
            }
        }
    }
}
