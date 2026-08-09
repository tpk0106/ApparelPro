using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyConversionService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/currencyConversion")]
    [ApiController]
    public class CurrencyConversionController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICurrencyConversionService _currencyConversionService;

        public CurrencyConversionController(IMapper mapper, ICurrencyConversionService currencyConversionService)
        {
            _mapper = mapper;
            _currencyConversionService = currencyConversionService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "currency-conversion-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<CurrencyConversionAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCurrencyConversionsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var currencyConversionServiceModels = await _currencyConversionService.GetCurrencyConversionsAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var currencyConversions = _mapper.Map<PaginationAPIModel<CurrencyConversionAPIModel>>(currencyConversionServiceModels);
            return Ok(currencyConversions);
        }

        [HttpGet("list/{fromCurrency}/{toCurrency}", Name = "GetCurrencyConversionByFromToAsync")]
        [Authorize(Policy = "currency-conversion-view")]
        [ProducesResponseType(typeof(CurrencyConversionAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetCurrencyConversionByFromToAsync(string fromCurrency, string toCurrency)
        {
            var currencyConversion = await _currencyConversionService.GetCurrencyConversionByFromToAsync(fromCurrency, toCurrency);
            if (currencyConversion == null)
            {
                return UnprocessableEntity($"No conversion rate found from {fromCurrency} to {toCurrency}.");
            }
            var currencyConversionAPIModel = _mapper.Map<CurrencyConversionAPIModel>(currencyConversion);
            return Ok(currencyConversionAPIModel);
        }

        [HttpPost]
        [Authorize(Policy = "currency-conversion-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> AddCurrencyConversionAsync([FromBody] CreateCurrencyConversionAPIModel createCurrencyConversionAPIModel)
        {
            var createCurrencyConversionServiceModel = _mapper.Map<CreateCurrencyConversionServiceModel>(createCurrencyConversionAPIModel);
            try
            {
                var addedCurrencyConversion = await _currencyConversionService.AddCurrencyConversionAsync(createCurrencyConversionServiceModel);
                var currencyConversionAPIModel = _mapper.Map<CurrencyConversionAPIModel>(addedCurrencyConversion);
                return CreatedAtRoute(nameof(GetCurrencyConversionByFromToAsync),
                    new { currencyConversionAPIModel.FromCurrency, currencyConversionAPIModel.ToCurrency }, currencyConversionAPIModel);
            }
            catch (InvalidOperationException ex)
            {
                // Add-time validation (missing Currency master codes, duplicate From/To pair,
                // same-currency pair) surfaces as a business-rule error, not a server fault -
                // same convention as GarmentTypeItemsService's SaveGarmentTypeItemAsync.
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(Policy = "currency-conversion-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> UpdateCurrencyConversionAsync([FromBody] UpdateCurrencyConversionAPIModel updateCurrencyConversionAPIModel)
        {
            var existing = await _currencyConversionService.GetCurrencyConversionByFromToAsync(
                updateCurrencyConversionAPIModel.FromCurrency, updateCurrencyConversionAPIModel.ToCurrency);
            if (existing == null)
            {
                return UnprocessableEntity($"No conversion rate found from {updateCurrencyConversionAPIModel.FromCurrency} to {updateCurrencyConversionAPIModel.ToCurrency}.");
            }

            var updateCurrencyConversionServiceModel = _mapper.Map<UpdateCurrencyConversionServiceModel>(updateCurrencyConversionAPIModel);
            await _currencyConversionService.UpdateCurrencyConversionAsync(updateCurrencyConversionServiceModel);
            return NoContent();
        }

        [HttpDelete("{fromCurrency}/{toCurrency}")]
        [Authorize(Policy = "currency-conversion-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteCurrencyConversionAsync(string fromCurrency, string toCurrency)
        {
            var existing = await _currencyConversionService.GetCurrencyConversionByFromToAsync(fromCurrency, toCurrency);
            if (existing == null)
            {
                return UnprocessableEntity($"No conversion rate found from {fromCurrency} to {toCurrency}.");
            }

            await _currencyConversionService.DeleteCurrencyConversionAsync(fromCurrency, toCurrency);
            return NoContent();
        }
    }
}
