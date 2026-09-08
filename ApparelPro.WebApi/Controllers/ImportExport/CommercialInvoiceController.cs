using apparelPro.BusinessLogic.Reports.ImportExport;
using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICommercialInvoiceService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/commercial-invoice")]
    [ApiController]
    public class CommercialInvoiceController : ControllerBase
    {
        private readonly ICommercialInvoiceService _commercialInvoiceService;
        private readonly IMapper _mapper;

        public CommercialInvoiceController(ICommercialInvoiceService commercialInvoiceService, IMapper mapper)
        {
            _commercialInvoiceService = commercialInvoiceService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "commercial-invoice-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<CommercialInvoiceHeaderAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetInvoicesAsync(
            [FromQuery] int pageSize, [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null, [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null, [FromQuery] string? filterQuery = null)
        {
            var result = await _commercialInvoiceService.GetInvoicesAsync(
                pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            return Ok(_mapper.Map<PaginationAPIModel<CommercialInvoiceHeaderAPIModel>>(result));
        }

        // GET: api/commercial-invoice/detail?invoiceNumber=34/GC/LG/94 - invoiceNumber
        // is a query param, not a route segment: real invoice numbers can contain
        // "/" (e.g. "34/GC/LG/94"), which a route segment would split into extra
        // path segments and 404.
        [HttpGet("detail")]
        [Authorize(Policy = "commercial-invoice-view")]
        [ProducesResponseType(typeof(CommercialInvoiceDetailAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByInvoiceNumberAsync([FromQuery] string invoiceNumber)
        {
            var detail = await _commercialInvoiceService.GetByInvoiceNumberAsync(invoiceNumber);
            if (detail == null) return NotFound();
            return Ok(_mapper.Map<CommercialInvoiceDetailAPIModel>(detail));
        }

        [HttpPut]
        [Authorize(Policy = "commercial-invoice-manage")]
        [ProducesResponseType(typeof(CommercialInvoiceDetailAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveCommercialInvoiceAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveCommercialInvoiceServiceModel>(apiModel);
            var saved = await _commercialInvoiceService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<CommercialInvoiceDetailAPIModel>(saved));
        }

        // DELETE: api/commercial-invoice?invoiceNumber=34/GC/LG/94
        [HttpDelete]
        [Authorize(Policy = "commercial-invoice-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromQuery] string invoiceNumber)
        {
            var deleted = await _commercialInvoiceService.DeleteAsync(invoiceNumber);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // GET: api/commercial-invoice/print/pdf?invoiceNumber=34/GC/LG/94&format=full|fedex|srilanka&printAssessmentNo=true
        [HttpGet("print/pdf")]
        [Authorize(Policy = "commercial-invoice-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> GetPrintPdfAsync(
            [FromQuery] string invoiceNumber, [FromQuery] string format = "full", [FromQuery] bool printAssessmentNo = true)
        {
            if (format is not ("full" or "fedex" or "srilanka"))
                return BadRequest($"Unknown print format '{format}'. Expected 'full', 'fedex', or 'srilanka'.");

            var details = await _commercialInvoiceService.GetPrintDetailsAsync(invoiceNumber);
            if (details == null) return NotFound();

            byte[] pdfBytes = format switch
            {
                "full" => CommercialInvoicePrintEngine.GenerateFullFormatPdf(details),
                "fedex" => CommercialInvoicePrintEngine.GenerateFedExStylePdf(details, printAssessmentNo),
                _ => CommercialInvoicePrintEngine.GenerateSriLankaCustomsPdf(details, printAssessmentNo),
            };

            // "/" in the invoice number (e.g. "34/GC/LG/94") is valid data but
            // not a valid filename character.
            var safeFileName = invoiceNumber.Replace('/', '-');
            var fileNamePrefix = format switch
            {
                "fedex" => "International_CommercialInvoice",
                "srilanka" => "SriLanka_CommercialInvoice",
                _ => "CommercialInvoice",
            };
            return File(pdfBytes, "application/pdf", $"{fileNamePrefix}_{safeFileName}.pdf");
        }
    }
}
