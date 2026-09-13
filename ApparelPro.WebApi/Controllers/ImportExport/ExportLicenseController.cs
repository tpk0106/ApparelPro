using apparelPro.BusinessLogic.Reports.ImportExport;
using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IExportLicenseService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/export-license")]
    [ApiController]
    public class ExportLicenseController : ControllerBase
    {
        private readonly IExportLicenseService _exportLicenseService;
        private readonly IMapper _mapper;

        public ExportLicenseController(IExportLicenseService exportLicenseService, IMapper mapper)
        {
            _exportLicenseService = exportLicenseService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "export-license-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<ExportLicenseHeaderAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetExportLicensesAsync(
            [FromQuery] int pageSize, [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null, [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null, [FromQuery] string? filterQuery = null)
        {
            var result = await _exportLicenseService.GetExportLicensesAsync(
                pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            return Ok(_mapper.Map<PaginationAPIModel<ExportLicenseHeaderAPIModel>>(result));
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "export-license-view")]
        [ProducesResponseType(typeof(ExportLicenseDetailAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var detail = await _exportLicenseService.GetByIdAsync(id);
            if (detail == null) return NotFound();
            return Ok(_mapper.Map<ExportLicenseDetailAPIModel>(detail));
        }

        [HttpPut]
        [Authorize(Policy = "export-license-manage")]
        [ProducesResponseType(typeof(ExportLicenseDetailAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveExportLicenseAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveExportLicenseServiceModel>(apiModel);
            var saved = await _exportLicenseService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<ExportLicenseDetailAPIModel>(saved));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "export-license-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var deleted = await _exportLicenseService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id:int}/print/pdf")]
        [Authorize(Policy = "export-license-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetPrintPdfAsync(int id)
        {
            var details = await _exportLicenseService.GetPrintDetailsAsync(id);
            if (details == null) return NotFound();

            var pdfBytes = ExportLicensePrintEngine.GeneratePdf(details);
            return File(pdfBytes, "application/pdf", $"ExportLicense_{id}.pdf");
        }
    }
}
