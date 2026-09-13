using apparelPro.BusinessLogic.Reports.ImportExport;
using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IValueDeclarationService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/value-declaration")]
    [ApiController]
    public class ValueDeclarationController : ControllerBase
    {
        private readonly IValueDeclarationService _valueDeclarationService;
        private readonly IMapper _mapper;

        public ValueDeclarationController(IValueDeclarationService valueDeclarationService, IMapper mapper)
        {
            _valueDeclarationService = valueDeclarationService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "value-declaration-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<ValueDeclarationHeaderAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetValueDeclarationsAsync(
            [FromQuery] int pageSize, [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null, [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null, [FromQuery] string? filterQuery = null)
        {
            var result = await _valueDeclarationService.GetValueDeclarationsAsync(
                pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            return Ok(_mapper.Map<PaginationAPIModel<ValueDeclarationHeaderAPIModel>>(result));
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "value-declaration-view")]
        [ProducesResponseType(typeof(ValueDeclarationDetailAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var detail = await _valueDeclarationService.GetByIdAsync(id);
            if (detail == null) return NotFound();
            return Ok(_mapper.Map<ValueDeclarationDetailAPIModel>(detail));
        }

        [HttpPut]
        [Authorize(Policy = "value-declaration-manage")]
        [ProducesResponseType(typeof(ValueDeclarationDetailAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveValueDeclarationAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveValueDeclarationServiceModel>(apiModel);
            var saved = await _valueDeclarationService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<ValueDeclarationDetailAPIModel>(saved));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "value-declaration-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var deleted = await _valueDeclarationService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id:int}/print/pdf")]
        [Authorize(Policy = "value-declaration-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetPrintPdfAsync(int id)
        {
            var details = await _valueDeclarationService.GetPrintDetailsAsync(id);
            if (details == null) return NotFound();

            var pdfBytes = ValueDeclarationPrintEngine.GeneratePdf(details);
            return File(pdfBytes, "application/pdf", $"ValueDeclaration_{details.Header.InvoiceNo.Replace('/', '-')}.pdf");
        }

        // Invoice-scoped flow (one Value Declaration per Commercial Invoice)
        // - additive, alongside the standalone endpoints above. invoiceNumber
        // is always a query param, never a route segment: real invoice
        // numbers can contain "/", same reasoning as Commercial Invoice.
        [HttpGet("detail")]
        [Authorize(Policy = "value-declaration-view")]
        [ProducesResponseType(typeof(ValueDeclarationDetailAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByInvoiceNumberAsync([FromQuery] string invoiceNumber)
        {
            var detail = await _valueDeclarationService.GetByInvoiceNumberAsync(invoiceNumber);
            if (detail == null) return NotFound();
            return Ok(_mapper.Map<ValueDeclarationDetailAPIModel>(detail));
        }

        [HttpPut("for-invoice")]
        [Authorize(Policy = "value-declaration-manage")]
        [ProducesResponseType(typeof(ValueDeclarationDetailAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveForInvoiceAsync([FromBody] SaveValueDeclarationAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveValueDeclarationServiceModel>(apiModel);
            var saved = await _valueDeclarationService.SaveForInvoiceAsync(serviceModel);
            return Ok(_mapper.Map<ValueDeclarationDetailAPIModel>(saved));
        }

        [HttpDelete("for-invoice")]
        [Authorize(Policy = "value-declaration-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteByInvoiceNumberAsync([FromQuery] string invoiceNumber)
        {
            var deleted = await _valueDeclarationService.DeleteByInvoiceNumberAsync(invoiceNumber);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("print/pdf/for-invoice")]
        [Authorize(Policy = "value-declaration-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetPrintPdfForInvoiceAsync([FromQuery] string invoiceNumber)
        {
            var details = await _valueDeclarationService.GetPrintDetailsByInvoiceNumberAsync(invoiceNumber);
            if (details == null) return NotFound();

            var pdfBytes = ValueDeclarationPrintEngine.GeneratePdf(details);
            var safeFileName = invoiceNumber.Replace('/', '-');
            return File(pdfBytes, "application/pdf", $"ValueDeclaration_{safeFileName}.pdf");
        }
    }
}
