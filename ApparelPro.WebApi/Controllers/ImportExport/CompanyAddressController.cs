using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICompanyAddressService;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/import-export/company-addresses")]
    [ApiController]
    public class CompanyAddressController : ControllerBase
    {
        private readonly ICompanyAddressService _companyAddressService;
        private readonly IMapper _mapper;

        public CompanyAddressController(ICompanyAddressService companyAddressService, IMapper mapper)
        {
            _companyAddressService = companyAddressService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "company-address-view")]
        [ProducesResponseType(typeof(List<CompanyAddressAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCompanyAddressesAsync()
        {
            var serviceModels = await _companyAddressService.GetAllAsync();
            return Ok(_mapper.Map<List<CompanyAddressAPIModel>>(serviceModels));
        }

        [HttpPut]
        [Authorize(Policy = "company-address-manage")]
        [ProducesResponseType(typeof(CompanyAddressAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveCompanyAddressAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SaveCompanyAddressServiceModel>(apiModel);
            var saved = await _companyAddressService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<CompanyAddressAPIModel>(saved));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "company-address-manage")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            var deleted = await _companyAddressService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }
    }
}
