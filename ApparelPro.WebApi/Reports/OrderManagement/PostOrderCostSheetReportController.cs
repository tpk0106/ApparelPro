using apparelPro.BusinessLogic.Reports.OrderManagement.PostOrderCostSheetReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_PCOST.PRG's "POST ORDER COST SHEET" - Reports -> Order Management ->
    // Post Order Cost Sheet in this app's nav. Percent/Freight/ActualShippedDate mirror
    // legacy's own print-time prompts for these values.
    [Route("api/post-order-cost-sheet-report")]
    [ApiController]
    public class PostOrderCostSheetReportController : ControllerBase
    {
        private readonly IPostOrderCostSheetReportService _postOrderCostSheetReportService;
        private readonly IMapper _mapper;

        public PostOrderCostSheetReportController(IPostOrderCostSheetReportService postOrderCostSheetReportService, IMapper mapper)
        {
            _postOrderCostSheetReportService = postOrderCostSheetReportService;
            _mapper = mapper;
        }

        // GET: api/post-order-cost-sheet-report/details?buyerCode=&order=&percentOfTotalValue=&freightCharges=&actualShippedDate=
        [HttpGet("details")]
        [Authorize(Policy = "post-order-cost-sheet-report")]
        public async Task<IActionResult> GetDetails(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] decimal percentOfTotalValue = 0,
            [FromQuery] decimal freightCharges = 0,
            [FromQuery] DateTime? actualShippedDate = null)
        {
            try
            {
                var report = await _postOrderCostSheetReportService.GetPostOrderCostSheetReportAsync(
                    buyerCode, order, percentOfTotalValue, freightCharges, actualShippedDate);
                var apiModel = _mapper.Map<PostOrderCostSheetReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Post Order Cost Sheet Report: {ex.Message}" });
            }
        }

        // GET: api/post-order-cost-sheet-report/pdf?buyerCode=&order=&percentOfTotalValue=&freightCharges=&actualShippedDate=
        [HttpGet("pdf")]
        [Authorize(Policy = "post-order-cost-sheet-report")]
        public async Task<IActionResult> GetPdf(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] decimal percentOfTotalValue = 0,
            [FromQuery] decimal freightCharges = 0,
            [FromQuery] DateTime? actualShippedDate = null)
        {
            try
            {
                var report = await _postOrderCostSheetReportService.GetPostOrderCostSheetReportAsync(
                    buyerCode, order, percentOfTotalValue, freightCharges, actualShippedDate);
                byte[] pdfBytes = PostOrderCostSheetReportEngine.GeneratePdf(report);
                string safeFileName = $"PostOrderCostSheetReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Post Order Cost Sheet Report PDF document: {ex.Message}" });
            }
        }
    }
}
