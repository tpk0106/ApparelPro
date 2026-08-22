using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.INonProductiveHourCodeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/non-productive-hour-code")]
    [ApiController]
    public class NonProductiveHourCodeController : ControllerBase
    {
        private readonly INonProductiveHourCodeService _nonProductiveHourCodeService;
        private readonly IMapper _mapper;

        public NonProductiveHourCodeController(INonProductiveHourCodeService nonProductiveHourCodeService, IMapper mapper)
        {
            _nonProductiveHourCodeService = nonProductiveHourCodeService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "non-productive-hour-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<NonProductiveHourCodeAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetNonProductiveHourCodesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _nonProductiveHourCodeService.GetNonProductiveHourCodesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var itemsPage = _mapper.Map<PaginationAPIModel<NonProductiveHourCodeAPIModel>>(serviceModels);
            return Ok(itemsPage);
        }

        [HttpGet("list/{code}", Name = "GetNonProductiveHourCodeByCodeAsync")]
        [Authorize(Policy = "non-productive-hour-view")]
        [ProducesResponseType(typeof(NonProductiveHourCodeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetNonProductiveHourCodeByCodeAsync(string code)
        {
            var nph = await _nonProductiveHourCodeService.GetNonProductiveHourCodeByCodeAsync(code);
            if (nph == null)
            {
                return UnprocessableEntity("Non-productive hour code is not available for code :" + code);
            }
            return Ok(_mapper.Map<NonProductiveHourCodeAPIModel>(nph));
        }

        [HttpPost]
        [Authorize(Policy = "non-productive-hour-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddNonProductiveHourCodeAsync([FromBody] CreateNonProductiveHourCodeAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateNonProductiveHourCodeServiceModel>(createAPIModel);
            var added = await _nonProductiveHourCodeService.AddNonProductiveHourCodeAsync(createServiceModel);
            return CreatedAtRoute(nameof(GetNonProductiveHourCodeByCodeAsync), new { code = added.Code }, null);
        }

        [HttpPut]
        [Authorize(Policy = "non-productive-hour-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateNonProductiveHourCodeAsync([FromQuery] string code, [FromBody] UpdateNonProductiveHourCodeAPIModel updateAPIModel)
        {
            var existing = await _nonProductiveHourCodeService.GetNonProductiveHourCodeByCodeAsync(code);
            if (existing == null)
            {
                return UnprocessableEntity("Non-productive hour code is not available for code :" + code);
            }
            updateAPIModel.Code = code;
            var updateServiceModel = _mapper.Map<UpdateNonProductiveHourCodeServiceModel>(updateAPIModel);
            await _nonProductiveHourCodeService.UpdateNonProductiveHourCodeAsync(updateServiceModel);
            return NoContent();
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "non-productive-hour-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteNonProductiveHourCodeAsync(string code)
        {
            var existing = await _nonProductiveHourCodeService.GetNonProductiveHourCodeByCodeAsync(code);
            if (existing == null)
            {
                return UnprocessableEntity("Non-productive hour code is not available for code :" + code);
            }
            await _nonProductiveHourCodeService.DeleteNonProductiveHourCodeAsync(code);
            return NoContent();
        }
    }
}
