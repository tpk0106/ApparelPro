using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/currency")]
    [ApiController]
    [Authorize("RegisteredUser")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ICountryService _countryService;
        private readonly IMapper _mapper;

        public CurrencyController(ICurrencyService currencyService, IMapper mapper, ICountryService countryService)
        {
            _currencyService = currencyService;
            _mapper = mapper;   
            _countryService = countryService;
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(PaginationAPIModel<CurrencyAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCurrenciesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            var currencyServiceModels = await _currencyService.GetCurrenciesAsync(pageNumber, pageSize, sortColumn,sortOrder, filterColumn, filterQuery);
            var currencies = _mapper.Map<PaginationAPIModel<CurrencyAPIModel>>(currencyServiceModels);            
            return Ok(currencies);
        }

        [HttpGet("list/{code}", Name = "GetCurrencyByCodeAsync")]
        [ProducesResponseType(typeof(CurrencyAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetCurrencyByCodeAsync(string code)
        {
            //Response.Headers.AccessControlAllowOrigin = "*";
            var currency = await _currencyService.GetCurrencyByCodeAsync(code);
            if (currency == null)
            {
                return UnprocessableEntity("currency is not available for code :" + code);
            }
            var currencyAPIModel = _mapper.Map<CurrencyAPIModel>(currency);
            return Ok(currencyAPIModel);
        }

        [HttpDelete("{code}")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteCurrencyAsync(string code)
        {
            var currency = await _currencyService.GetCurrencyByCodeAsync(code);
            if (currency == null)
            {
                return UnprocessableEntity("Currency is not available for code :" + code);
            }
            await _currencyService.DeleteCurrencyAsync(code);
            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddCurrencyAsync([FromBody] CreateCurrencyAPIModel createCurrencyAPIModel)
        {
            Response.Headers.AccessControlAllowOrigin = "*";
            var existingCurrency = await _currencyService.GetCurrencyByCodeAsync(createCurrencyAPIModel!.Code);
            if (existingCurrency != null)
            {
                return UnprocessableEntity("Currency is already available for code :" + createCurrencyAPIModel!.Code);
            }
            var createCurrencyServiceModel = _mapper.Map<CreateCurrencyServiceModel>(createCurrencyAPIModel);
            var adddedCurrency = await _currencyService.AddCurrencyAsync(createCurrencyServiceModel);
            //var categoryAPIModel = _mapper.Map<CategoryAPIModel>(addedCategory);
            return CreatedAtRoute(nameof(GetCurrencyByCodeAsync), new { adddedCurrency.Code }, null);
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateCurrencyAsync([FromQuery] string code, [FromBody] UpdateCurrencyAPIModel
            updateCurrencyAPIModel)
        {
            var resultCurrencyAPIModel = _mapper.Map<CurrencyAPIModel>(await _currencyService.GetCurrencyByCodeAsync(code));
            if (resultCurrencyAPIModel == null)
            {
                return UnprocessableEntity("Currency is not available for code :" + code);
            }
            var updateCurrencySeviceModel = _mapper.Map<UpdateCurrencyServiceModel>(updateCurrencyAPIModel);
            await _currencyService.UpdateCurrencyAsync(updateCurrencySeviceModel);
            return NoContent();
        }


        [HttpGet("list/does-currency-exist/{code}-{countryCode}", Name = "DoesCurrencyExistAsync")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DoesCurrencyExistAsync(string code, string countryCode)
        {
            var exist = await _currencyService.DoesCurrencyExistAsync(code, countryCode);
            return Ok(exist);
        }
    }
}
