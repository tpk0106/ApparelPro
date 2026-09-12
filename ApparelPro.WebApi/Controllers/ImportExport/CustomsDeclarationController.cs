using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsDeclarationService;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/customs-declaration")]
    [ApiController]
    public class CustomsDeclarationController : ControllerBase
    {
        private readonly ICustomsDeclarationService _customsDeclarationService;
        private readonly IMapper _mapper;

        public CustomsDeclarationController(ICustomsDeclarationService customsDeclarationService, IMapper mapper)
        {
            _customsDeclarationService = customsDeclarationService;
            _mapper = mapper;
        }

        // GET: api/customs-declaration/detail?cusNo=...
        [HttpGet("detail")]
        [Authorize(Policy = "customs-declaration-view")]
        [ProducesResponseType(typeof(CustomsDeclarationDetailAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetByCusNoAsync([FromQuery] string cusNo)
        {
            var detail = await _customsDeclarationService.GetByCusNoAsync(cusNo);
            if (detail == null) return NotFound();
            return Ok(_mapper.Map<CustomsDeclarationDetailAPIModel>(detail));
        }

        [HttpPut]
        [Authorize(Policy = "customs-declaration-manage")]
        [ProducesResponseType(typeof(CustomsDeclarationDetailAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveCustomsDeclarationAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveCustomsDeclarationServiceModel>(apiModel);
            var saved = await _customsDeclarationService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<CustomsDeclarationDetailAPIModel>(saved));
        }

        // DELETE: api/customs-declaration?cusNo=...
        [HttpDelete]
        [Authorize(Policy = "customs-declaration-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromQuery] string cusNo)
        {
            var deleted = await _customsDeclarationService.DeleteAsync(cusNo);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
