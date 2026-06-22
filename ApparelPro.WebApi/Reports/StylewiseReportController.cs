using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.Reports.Models;
using ApparelPro.WebApi.Reports.OrderManagement.Stylewise_Events;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApparelPro.WebApi.Reports
{
    [Route("api/stylewise-reports")]
    [ApiController]
    public class StylewiseReportController : ControllerBase
    {
        //private readonly IStylewiseReportService _stylewiseReportService;
        private readonly IStylewiseEventService _stylewiseEventService;
        private readonly IStyleDetailsService _styleDetailsService;
        private readonly IMapper _mapper;
        public StylewiseReportController(
            //IStylewiseReportService stylewiseReportService,
            IStylewiseEventService stylewiseEventService,
            IStyleDetailsService styleDetailsService, IMapper mapper)
        {
            //_stylewiseReportService = stylewiseReportService;
            _stylewiseEventService = stylewiseEventService;
            _styleDetailsService = styleDetailsService;
            _mapper = mapper;
        }

        // GET: api/stylewise-event/print-report?buyerCode=2&order=1017-18&typeCode=2&styleCode=M102
        [HttpGet("print-report")]
        public async Task<IActionResult> PrintReport(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(styleCode))
                return BadRequest("Tracking reference parameters cannot be empty.");

            try
            {
                // 1. Pull the active, auto-initialized milestone dataset array rows straight from your service layer
                var dataset = await _stylewiseEventService.GetStyleEventsAsync(buyerCode, order, typeCode, styleCode);

                if (!dataset.Any()) return NotFound("No event milestone details found to extract for this style context selection.");

                // 2. Fetch style row variables to search for manager verification lock parameters
                var styleApprovalDetailsServiceModel = await _styleDetailsService.GetEstimateApprovalUserNameAsync(buyerCode, order, typeCode, styleCode);
                var styleHeader = _mapper.Map<StyleApprovalDetailsAPIModel>(styleApprovalDetailsServiceModel);
                
                string approvalStamp = "";
                if (styleHeader != null && !string.IsNullOrEmpty(styleHeader.EstimateApprovalUserName))
                {
                    // Replicates the 'Approved by Manager on Date' label block from your Clipper code report tail
                    approvalStamp = $"✓ Officially Signed-off and Approved by Manager ID: {styleHeader.EstimateApprovalUserName} on {styleHeader.EstimateApprovalDate?.ToString("dd-MMM-yyyy")}";
                }

                // 3. Compile the array parameters using your vector graphics PDF constructor engine
                byte[] pdfBytes = StylewiseEventReportEngine.GenerateCriticalPathPdf(dataset, approvalStamp);

                // 4. Return the complete application stream file payload cleanly to download straight to client monitors
                string safeFileName = $"CriticalPathReport_{styleCode.Trim()}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", safeFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to construct operational PDF document stream: {ex.Message}" });
            }
        }
    }
}
