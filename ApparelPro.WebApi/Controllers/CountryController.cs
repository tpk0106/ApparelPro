using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/country")]
    [ApiController]
  //  [Authorize("RegisteredUser")]
    public class CountryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICountryService _countryService;
        //private readonly ApplicationParams _applicationParams; 
        public CountryController(ICountryService countryService, IMapper  mapper)
        {
            if(countryService == null) throw new ArgumentNullException(nameof(countryService));
            if(mapper == null) throw new ArgumentNullException(nameof(mapper));
         
            _countryService = countryService;
            _mapper = mapper;
        }

        [HttpGet("list")]
      //  [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied
        //[Authorize(Roles = "Inventory")]
       // [Authorize("RegisteredUser")]
        [ProducesResponseType(typeof(PaginationAPIModel<CountryAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCountriesAsync(
            [FromQuery] int pageSize, 
            [FromQuery] int pageNumber, 
            [FromQuery] string? sortColumn = null, 
            [FromQuery] string? sortOrder =null, 
            [FromQuery] string? filterColumn = null, 
            [FromQuery] string? filterQuery = null)
        {         
            var countryServiceModels = await _countryService.GetCountriesAsync(pageNumber,pageSize, 
                sortColumn,sortOrder,filterColumn,filterQuery);
            var countries = _mapper.Map<PaginationAPIModel<CountryAPIModel>>(countryServiceModels);
            return Ok(countries);
        }

        [HttpGet("list/{code}", Name = "GetCountryByCodeAsync")]
        [ProducesResponseType(typeof(CountryAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetCountryByCodeAsync(string code)
        {          
            var country = await _countryService.GetCountryByCodeAsync(code);
            if (country == null)
            {
                return UnprocessableEntity("country is not available for code :" + code);
            }
            var countryAPIModel = _mapper.Map<CountryAPIModel>(country);
            return Ok(countryAPIModel);
        }

        [HttpGet("list/does-country-exist/{code}", Name = "DoesCountryExistAsync")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]       
        public async Task<IActionResult> DoesCountryExistAsync(string code)
        {
            var exist = await _countryService.DoesCountryExistAsync(code);            
            return Ok(exist);
        }


        [HttpGet("list/paging/{pageSize}-{pageNumber}", Name = "GetCountryByPageNumberAsync")]
        [ProducesResponseType(typeof(IEnumerable<CountryAPIModel>), HttpStatusCodes.OK)]        
        public async Task<IActionResult> GetCountryByPageNumberAsync(int pageNumber, int pageSize)
        {           
            var filteredCountries = await _countryService.GetCountriesByPageNumberAsync(pageNumber,pageSize);           
            var countryAPIModels = _mapper.Map<IEnumerable<CountryAPIModel>>(filteredCountries);
            return Ok(countryAPIModels);
        }

        [HttpGet("list/filter/{pageSize}-{pageNumber}", Name = "FilterCountriesByCodeAsync")]
        [ProducesResponseType(typeof(IEnumerable<CountryAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> FilterCountriesByCodeAsync([FromQuery] string? filter, int pageNumber=1, int pageSize = 10)
        {            
            if(pageSize == 0)
            {
                pageSize = ApplicationParams.PageSize;
            }
            
            var filteredCountries = await _countryService.FilterCountriesByCodeAsync(filter, pageNumber, pageSize);
            var countryAPIModels = _mapper.Map<IEnumerable<CountryAPIModel>>(filteredCountries);
            return Ok(countryAPIModels);
        }

        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddCountryAsync([FromBody] CreateCountryAPIModel createCountryAPIModel )
        {
            var createCountryServiceModel = _mapper.Map<CreateCountryServiceModel>(createCountryAPIModel);
            var addedCountry = await _countryService.AddCountryAsync(createCountryServiceModel);
            return CreatedAtRoute(nameof(GetCountryByCodeAsync), new { code = addedCountry.Code }, null);
        }

        [HttpDelete("{code}")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteCountryAsync(string code)
        {
            var country = await _countryService.GetCountryByCodeAsync(code);
            if (country == null)
            {
                return UnprocessableEntity("Country is not available for code :" + code);
            }
            await _countryService.DeleteCountryAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
      //  [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCountryAsync([FromQuery] string code, [FromBody] UpdateCountryAPIModel  
            updateCountryAPIModel)
        {
            var resultCountryAPIModel = _mapper.Map<CountryAPIModel>(await _countryService.GetCountryByCodeAsync(code));
           // Response.Headers.AccessControlAllowOrigin = "*";
            if (resultCountryAPIModel == null)
            {
                return UnprocessableEntity("Country is not available for code :" + code);
            }
            updateCountryAPIModel.Id = resultCountryAPIModel.Id;
            var updateCountrySeviceModel = _mapper.Map<UpdateCountryServiceModel>(updateCountryAPIModel);
            await _countryService.UpdateCountryAsync(updateCountrySeviceModel);
            return NoContent();
        }

        [HttpPatch()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateCountryAsync()
        {        
            return NoContent();
        }
    }
}
