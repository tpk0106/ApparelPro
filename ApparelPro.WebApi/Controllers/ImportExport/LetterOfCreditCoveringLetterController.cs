using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditCoveringLetterService;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/letter-of-credit-covering-letter")]
    [ApiController]
    public class LetterOfCreditCoveringLetterController : ControllerBase
    {
        private readonly ILetterOfCreditCoveringLetterService _letterOfCreditCoveringLetterService;
        private readonly IMapper _mapper;

        public LetterOfCreditCoveringLetterController(
            ILetterOfCreditCoveringLetterService letterOfCreditCoveringLetterService, IMapper mapper)
        {
            _letterOfCreditCoveringLetterService = letterOfCreditCoveringLetterService;
            _mapper = mapper;
        }

        // GET: api/letter-of-credit-covering-letter/detail?bankCode=BOC&lcNo=...
        [HttpGet("detail")]
        [Authorize(Policy = "letter-of-credit-view")]
        [ProducesResponseType(typeof(LetterOfCreditCoveringLetterAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByKeyAsync([FromQuery] string bankCode, [FromQuery] string lcNo)
        {
            var serviceModel = await _letterOfCreditCoveringLetterService.GetByKeyAsync(bankCode, lcNo);
            if (serviceModel == null) return NotFound();
            return Ok(_mapper.Map<LetterOfCreditCoveringLetterAPIModel>(serviceModel));
        }

        [HttpPut]
        [Authorize(Policy = "letter-of-credit-manage")]
        [ProducesResponseType(typeof(LetterOfCreditCoveringLetterAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] LetterOfCreditCoveringLetterAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<LetterOfCreditCoveringLetterServiceModel>(apiModel);
            var saved = await _letterOfCreditCoveringLetterService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<LetterOfCreditCoveringLetterAPIModel>(saved));
        }

        // DELETE: api/letter-of-credit-covering-letter?bankCode=BOC&lcNo=...
        [HttpDelete]
        [Authorize(Policy = "letter-of-credit-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromQuery] string bankCode, [FromQuery] string lcNo)
        {
            var deleted = await _letterOfCreditCoveringLetterService.DeleteAsync(bankCode, lcNo);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
