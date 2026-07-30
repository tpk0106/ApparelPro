using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyExchangeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/currencyExchange")]
    [ApiController]
    public class CurrencyExchangeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        public CurrencyExchangeController(IMapper mapper, ICurrencyExchangeService currencyExchangeService)
        {
            _mapper = mapper;
            _currencyExchangeService = currencyExchangeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "currency-exchange-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<CurrencyExchangeAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCurrencyExchangesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var currencyExchangeServiceModels = await _currencyExchangeService.GetCurrencyExchangesAsync(pageNumber, pageSize, sortColumn , sortOrder , filterColumn , filterQuery);
            var currencyExchanges = _mapper.Map<PaginationAPIModel<CurrencyExchangeAPIModel>>(currencyExchangeServiceModels);
            return Ok(currencyExchanges);
        }

        [HttpGet("list/byDate")]
        [Authorize(Policy = "currency-exchange-view")]
        [ProducesResponseType(typeof(IEnumerable<CurrencyExchangeAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCurrencyExchangesByDateAsync()
        {
            var currencyExchangeServiceModels = await _currencyExchangeService.GetCurrencyExchangesByDateAsync();
            var currencyExchanges = _mapper.Map<IEnumerable<CurrencyExchangeAPIModel>>(currencyExchangeServiceModels);
            return Ok(currencyExchanges);
        }

        [HttpGet("list/{baseCurrency}", Name = "GetCurrencyExchangesByBaseCurrencyAsync")]
        [Authorize(Policy = "currency-exchange-view")]
        [ProducesResponseType(typeof(IEnumerable<CurrencyExchangeAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetCurrencyExchangesByBaseCurrencyAsync(string baseCurrency)
        {
            var currencyExchanges = await _currencyExchangeService.GetCurrencyExchangesByBaseCurrencyAsync(baseCurrency);
            var currencyExchangeAPIModels = _mapper.Map<IEnumerable<CurrencyExchangeAPIModel>>(currencyExchanges);
            return Ok(currencyExchangeAPIModels);
        }

        [HttpGet("list/{baseCurrency}/{quoteCurrency}/{date}", Name = "GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync")]
        [Authorize(Policy = "currency-exchange-view")]
        [ProducesResponseType(typeof(CurrencyExchangeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync(string baseCurrency, string quoteCurrency, DateTime date)
        {
            var currencyExchangeDbModel = await _currencyExchangeService
                .GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync(baseCurrency, quoteCurrency,date);
            var currencyExchangeAPIModel = _mapper.Map<CurrencyExchangeAPIModel>(currencyExchangeDbModel);
            return Ok(currencyExchangeAPIModel);
        }

        [HttpPost()]
        [Authorize(Policy = "currency-exchange-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddCurrencyExchangeAsync([FromBody] CreateCurrencyExchangeAPIModel createCurrencyExchangeAPIModel)
        {
            var existingCurrencyExchange = await _currencyExchangeService
                .GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync(
                baseCurrency: createCurrencyExchangeAPIModel!.BaseCurrency!,
                quoteCurrency: createCurrencyExchangeAPIModel!.QuoteCurrency!,
                date: createCurrencyExchangeAPIModel!.ExchangeDate);

            if (existingCurrencyExchange != null)
            {
                return UnprocessableEntity("Currency Exchange is already available for base currency :" + existingCurrencyExchange.BaseCurrency);
            }
            var createCurrencyServiceModel = _mapper.Map<CreateCurrencyExchangeServiceModel>(createCurrencyExchangeAPIModel);
            var addedCurrencyExchangeService = await _currencyExchangeService.AddCurrencyExchangeAsync(createCurrencyServiceModel);
            var currencyExchangeAPIModel = _mapper.Map<CurrencyExchangeAPIModel>(addedCurrencyExchangeService);
            return CreatedAtRoute(nameof(GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync), new { currencyExchangeAPIModel.BaseCurrency, currencyExchangeAPIModel.QuoteCurrency, currencyExchangeAPIModel.ExchangeDate}, null);
        }

        [HttpPut()]
        [Authorize(Policy = "currency-exchange-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult),
            HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateCurrencyExchangeAsync(
            [FromQuery] string baseCurrency,
            [FromQuery] string quoteCurrency,
            [FromQuery] DateTime exchangeDate,
            [FromBody] UpdateCurrencyExchangeAPIModel
           updateCurrencyExchangeAPIModel)
        {
            var resultCurrencyExchangeAPIModel = _mapper.Map<UpdateCurrencyExchangeAPIModel>(
                await _currencyExchangeService.GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync(baseCurrency,quoteCurrency,exchangeDate));
            if (resultCurrencyExchangeAPIModel == null)
            {
                return UnprocessableEntity("Currency is not available for base currency code :" + baseCurrency);
            }
            var updateCurrencyExchangeSeviceModel = _mapper.Map<UpdateCurrencyExchangeServiceModel>(updateCurrencyExchangeAPIModel);
            await _currencyExchangeService.UpdateCurrencyExchangeAsync(updateCurrencyExchangeSeviceModel);
            return NoContent();
        }

        [HttpDelete("{baseCurrency}/{quoteCurrency}/{exchangeDate}")]
        [Authorize(Policy = "currency-exchange-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteCurrencyExchangeAsync([FromRoute] string baseCurrency, [FromRoute]string quoteCurrency, [FromRoute] DateTime exchangeDate)
        {
            var currencyExchangeServiceModel = await _currencyExchangeService.GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync(baseCurrency,quoteCurrency, exchangeDate);
            if (currencyExchangeServiceModel == null)
            {
                return UnprocessableEntity("Currency Exchange is not available for base currency :" + baseCurrency);
            }
            await _currencyExchangeService.DeleteCurrencyExchangeAsync(baseCurrency,quoteCurrency,exchangeDate);
            return NoContent();
        }
    }
}
