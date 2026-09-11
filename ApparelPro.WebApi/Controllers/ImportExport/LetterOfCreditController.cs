using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditService;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/letter-of-credit")]
    [ApiController]
    public class LetterOfCreditController : ControllerBase
    {
        private readonly ILetterOfCreditService _letterOfCreditService;
        private readonly IMapper _mapper;

        public LetterOfCreditController(ILetterOfCreditService letterOfCreditService, IMapper mapper)
        {
            _letterOfCreditService = letterOfCreditService;
            _mapper = mapper;
        }

        // GET: api/letter-of-credit/detail?bankCode=BOC&lcNo=...
        [HttpGet("detail")]
        [Authorize(Policy = "letter-of-credit-view")]
        [ProducesResponseType(typeof(LetterOfCreditDetailAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByKeyAsync([FromQuery] string bankCode, [FromQuery] string lcNo)
        {
            var detail = await _letterOfCreditService.GetByKeyAsync(bankCode, lcNo);
            if (detail == null) return NotFound();
            return Ok(_mapper.Map<LetterOfCreditDetailAPIModel>(detail));
        }

        [HttpPut]
        [Authorize(Policy = "letter-of-credit-manage")]
        [ProducesResponseType(typeof(LetterOfCreditDetailAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveLetterOfCreditAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveLetterOfCreditServiceModel>(apiModel);
            var saved = await _letterOfCreditService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<LetterOfCreditDetailAPIModel>(saved));
        }

        // DELETE: api/letter-of-credit?bankCode=BOC&lcNo=...
        [HttpDelete]
        [Authorize(Policy = "letter-of-credit-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromQuery] string bankCode, [FromQuery] string lcNo)
        {
            var deleted = await _letterOfCreditService.DeleteAsync(bankCode, lcNo);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
