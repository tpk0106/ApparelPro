using apparelPro.BusinessLogic.Reports.ImportExport;
using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IBoatNoteService;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/boat-note")]
    [ApiController]
    public class BoatNoteController : ControllerBase
    {
        private readonly IBoatNoteService _boatNoteService;
        private readonly IMapper _mapper;

        public BoatNoteController(IBoatNoteService boatNoteService, IMapper mapper)
        {
            _boatNoteService = boatNoteService;
            _mapper = mapper;
        }

        // GET: api/boat-note/detail?invoiceNumber=34/GC/LG/94 - query param,
        // not a route segment, same reasoning as the other IE docs: real
        // invoice numbers can contain "/".
        [HttpGet("detail")]
        [Authorize(Policy = "boat-note-view")]
        [ProducesResponseType(typeof(BoatNoteDetailAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByInvoiceNumberAsync([FromQuery] string invoiceNumber)
        {
            var detail = await _boatNoteService.GetByInvoiceNumberAsync(invoiceNumber);
            if (detail == null) return NotFound();
            return Ok(_mapper.Map<BoatNoteDetailAPIModel>(detail));
        }

        [HttpPut]
        [Authorize(Policy = "boat-note-manage")]
        [ProducesResponseType(typeof(BoatNoteDetailAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveBoatNoteAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveBoatNoteServiceModel>(apiModel);
            var saved = await _boatNoteService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<BoatNoteDetailAPIModel>(saved));
        }

        // DELETE: api/boat-note?invoiceNumber=34/GC/LG/94
        [HttpDelete]
        [Authorize(Policy = "boat-note-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromQuery] string invoiceNumber)
        {
            var deleted = await _boatNoteService.DeleteAsync(invoiceNumber);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // GET: api/boat-note/print/pdf?invoiceNumber=34/GC/LG/94
        [HttpGet("print/pdf")]
        [Authorize(Policy = "boat-note-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetPrintPdfAsync([FromQuery] string invoiceNumber)
        {
            var details = await _boatNoteService.GetPrintDetailsAsync(invoiceNumber);
            if (details == null) return NotFound();

            var pdfBytes = BoatNotePrintEngine.GeneratePdf(details);
            var safeFileName = invoiceNumber.Replace('/', '-');
            return File(pdfBytes, "application/pdf", $"BoatNote_{safeFileName}.pdf");
        }
    }
}
