using apparelPro.BusinessLogic.Reports.OrderManagement.CostOfProductionReport;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Reports.OrderManagement
{
    // Replicates OD_FCOST.PRG's "COST OF PRODUCTION" - Reports -> Order Management ->
    // Cost of Production in this app's nav.
    [Route("api/cost-of-production-report")]
    [ApiController]
    public class CostOfProductionReportController : ControllerBase
    {
        private readonly ICostOfProductionReportService _costOfProductionReportService;
        private readonly IMapper _mapper;

        public CostOfProductionReportController(ICostOfProductionReportService costOfProductionReportService, IMapper mapper)
        {
            _costOfProductionReportService = costOfProductionReportService;
            _mapper = mapper;
        }

        // GET: api/cost-of-production-report/details?buyerCode=&order=
        [HttpGet("details")]
        [Authorize(Policy = "cost-of-production-report")]
        public async Task<IActionResult> GetDetails([FromQuery] int buyerCode, [FromQuery] string order)
        {
            try
            {
                var report = await _costOfProductionReportService.GetCostOfProductionReportAsync(buyerCode, order);
                var apiModel = _mapper.Map<CostOfProductionReportAPIModel>(report);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to compile Cost of Production Report: {ex.Message}" });
            }
        }

        // GET: api/cost-of-production-report/pdf?buyerCode=&order=
        [HttpGet("pdf")]
        [Authorize(Policy = "cost-of-production-report")]
        public async Task<IActionResult> GetPdf([FromQuery] int buyerCode, [FromQuery] string order)
        {
            try
            {
                var report = await _costOfProductionReportService.GetCostOfProductionReportAsync(buyerCode, order);
                byte[] pdfBytes = CostOfProductionReportEngine.GeneratePdf(report);
                string safeFileName = $"CostOfProductionReport_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct Cost of Production Report PDF document: {ex.Message}" });
            }
        }
    }
}
