using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/unit-conversions")]
    [ApiController]
    public class UnitConversionController : ControllerBase
    {
        private readonly IUnitConversionService _unitConversion;
        private readonly IMapper _mapper;
        public UnitConversionController(IUnitConversionService unitConversionService, IMapper mapper)
        {
            _unitConversion = unitConversionService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "unit-conversion-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<UnitConversionAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCountriesAsync(
          [FromQuery] int pageSize,
          [FromQuery] int pageNumber,
          [FromQuery] string? sortColumn = null,
          [FromQuery] string? sortOrder = null,
          [FromQuery] string? filterColumn = null,
          [FromQuery] string? filterQuery = null)
        {
            var unitConversionModels = await _unitConversion.GetUnitConversionsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var units = _mapper.Map<PaginationAPIModel<UnitConversionAPIModel>>(unitConversionModels);
            return Ok(units);
        }

    }
}
