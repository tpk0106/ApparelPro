using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/Country")]
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
        //[Authorize("Merchandising")] // policy applied
        //[Authorize(Roles = "Inventory")]
        // [Authorize("RegisteredUser")]
        [Authorize(Roles = "Merchandiser, Merchandiser Manager")]
        [SwaggerOperation(Tags = new[] { "Country Endpoints" },
            Summary = "list all Country details with paging and filtering.",
            Description = "Returns 200 - OK with PaginationAPIModel with Country list.")
        ]
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
        [SwaggerOperation(Tags = new[] { "Country Endpoints" },
            Summary = "list a Country details for a given Country Code",
            Description = "Returns 200 - OK with Country model.")
        ]
        public async Task<IActionResult> GetCountryByCodeAsync(string code)
        {          
            var Country = await _countryService.GetCountryByCodeAsync(code);
            if (Country == null)
            {
                return UnprocessableEntity("Country is not available for code :" + code);
            }
            var countryAPIModel = _mapper.Map<CountryAPIModel>(Country);
            return Ok(countryAPIModel);
        }

        [HttpGet("list/does-Country-exist/{code}", Name = "DoesCountryExistAsync")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Country Endpoints" },
            Summary = "check whether the given Country exist.",
            Description = "Returns 200 - OK with Country model.")
        ]
        public async Task<IActionResult> DoesCountryExistAsync(string code)
        {
            var exist = await _countryService.DoesCountryExistAsync(code);            
            return Ok(exist);
        }

        [SwaggerOperation(Tags = new[] { "Country Endpoints" },
           Summary = "list a Country details for a given Country Code",
           Description = "Returns 200 - OK with Country model.")
       ]

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
        [SwaggerOperation(Tags = new[] { "Country Endpoints" },
            Summary = "list a Country details for a given Country Code",
            Description = "Returns 200 - OK with Country model.")
        ]
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

        [HttpPost()]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        [SwaggerOperation(Tags = new[] { "Country Endpoints" },
            Summary = "Add a Country.",
            Description = "Returns 200 - OK with No content and 400 with Bad Request")
        ]
        public async Task<IActionResult> AddCountryAsync([FromBody] CreateCountryAPIModel createCountryAPIModel )
        {
            try
            {
                var createCountryServiceModel = _mapper.Map<CreateCountryServiceModel>(createCountryAPIModel);
                var addedCountry = await _countryService.AddCountryAsync(createCountryServiceModel);
                return CreatedAtRoute(nameof(GetCountryByCodeAsync), new { code = addedCountry.Code }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
          
        }

//
        [HttpDelete()]
        //[HttpDelete("{code}")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Country Endpoints" },
            Summary = "Delete a Country.",
            Description = "Returns 200 - OK with No content")
        ]      
        public async Task<IActionResult> DeleteCountryAsync([FromQuery] string code)
        {
            var Country = await _countryService.GetCountryByCodeAsync(code);
            if (Country == null)
            {
                return UnprocessableEntity("Country is not available for code :" + code);
            }
            await _countryService.DeleteCountryAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Country Endpoints" },
            Summary = "Update a Country.",
            Description = "Returns 200 - OK with No content")
        ]
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
