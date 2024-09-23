using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.Reference;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/currencyConversion")]
    [Authorize("RegisteredUser")]
    [ApiController]
    public class CurrencyConversionController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICurrencyConversionService _currencyConversionService;
        public CurrencyConversionController(IMapper mapper, ICurrencyConversionService currencyConversionService)
        {
            _currencyConversionService = currencyConversionService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize("Merchandising")] // policy applied
        [ProducesResponseType(typeof(PaginationAPIModel<CurrencyExchangeAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCurrencyConversionsAsync(
           [FromQuery] int pageSize,
           [FromQuery] int pageNumber,
           [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
           [FromQuery] string? filterQuery = null)
        {
            var currencyExchangeServiceModels = await _currencyConversionService.GetCurrencyConversionsAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
          //  var currencyExchanges = _mapper.Map<PaginationAPIModel<CurrencyConversionAPIModel>>(currencyExchangeServiceModels);
            return Ok();
        }
    }
}
