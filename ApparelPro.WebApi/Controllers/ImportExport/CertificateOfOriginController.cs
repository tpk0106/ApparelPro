using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICertificateOfOriginService;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/certificate-of-origin")]
    [ApiController]
    public class CertificateOfOriginController : ControllerBase
    {
        private readonly ICertificateOfOriginService _certificateOfOriginService;
        private readonly IMapper _mapper;

        public CertificateOfOriginController(ICertificateOfOriginService certificateOfOriginService, IMapper mapper)
        {
            _certificateOfOriginService = certificateOfOriginService;
            _mapper = mapper;
        }

        // GET: api/certificate-of-origin/detail?invoiceNumber=34/GC/LG/94 -
        // query param, not a route segment, same reasoning as Commercial
        // Invoice: real invoice numbers can contain "/".
        [HttpGet("detail")]
        [Authorize(Policy = "certificate-of-origin-view")]
        [ProducesResponseType(typeof(CertificateOfOriginDetailAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByInvoiceNumberAsync([FromQuery] string invoiceNumber)
        {
            var detail = await _certificateOfOriginService.GetByInvoiceNumberAsync(invoiceNumber);
            if (detail == null) return NotFound();
            return Ok(_mapper.Map<CertificateOfOriginDetailAPIModel>(detail));
        }

        [HttpPut]
        [Authorize(Policy = "certificate-of-origin-manage")]
        [ProducesResponseType(typeof(CertificateOfOriginDetailAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveCertificateOfOriginAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveCertificateOfOriginServiceModel>(apiModel);
            var saved = await _certificateOfOriginService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<CertificateOfOriginDetailAPIModel>(saved));
        }

        // DELETE: api/certificate-of-origin?invoiceNumber=34/GC/LG/94
        [HttpDelete]
        [Authorize(Policy = "certificate-of-origin-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromQuery] string invoiceNumber)
        {
            var deleted = await _certificateOfOriginService.DeleteAsync(invoiceNumber);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
