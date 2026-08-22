using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IGarmentComponentService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/garment-component")]
    [ApiController]
    public class GarmentComponentController : ControllerBase
    {
        private readonly IGarmentComponentService _garmentComponentService;
        private readonly IMapper _mapper;

        public GarmentComponentController(IGarmentComponentService garmentComponentService, IMapper mapper)
        {
            _garmentComponentService = garmentComponentService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "garment-component-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<GarmentComponentAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetGarmentComponentsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _garmentComponentService.GetGarmentComponentsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var itemsPage = _mapper.Map<PaginationAPIModel<GarmentComponentAPIModel>>(serviceModels);
            return Ok(itemsPage);
        }

        [HttpGet("list/{componentCode}", Name = "GetGarmentComponentByComponentCodeAsync")]
        [Authorize(Policy = "garment-component-view")]
        [ProducesResponseType(typeof(GarmentComponentAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetGarmentComponentByComponentCodeAsync(string componentCode)
        {
            var component = await _garmentComponentService.GetGarmentComponentByComponentCodeAsync(componentCode);
            if (component == null)
            {
                return UnprocessableEntity("Garment component is not available for code :" + componentCode);
            }
            return Ok(_mapper.Map<GarmentComponentAPIModel>(component));
        }

        [HttpPost]
        [Authorize(Policy = "garment-component-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddGarmentComponentAsync([FromBody] CreateGarmentComponentAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateGarmentComponentServiceModel>(createAPIModel);
            var added = await _garmentComponentService.AddGarmentComponentAsync(createServiceModel);
            return CreatedAtRoute(nameof(GetGarmentComponentByComponentCodeAsync), new { componentCode = added.ComponentCode }, null);
        }

        [HttpPut]
        [Authorize(Policy = "garment-component-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateGarmentComponentAsync([FromQuery] string componentCode, [FromBody] UpdateGarmentComponentAPIModel updateAPIModel)
        {
            var existing = await _garmentComponentService.GetGarmentComponentByComponentCodeAsync(componentCode);
            if (existing == null)
            {
                return UnprocessableEntity("Garment component is not available for code :" + componentCode);
            }
            updateAPIModel.ComponentCode = componentCode;
            var updateServiceModel = _mapper.Map<UpdateGarmentComponentServiceModel>(updateAPIModel);
            await _garmentComponentService.UpdateGarmentComponentAsync(updateServiceModel);
            return NoContent();
        }

        [HttpDelete("{componentCode}")]
        [Authorize(Policy = "garment-component-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteGarmentComponentAsync(string componentCode)
        {
            var existing = await _garmentComponentService.GetGarmentComponentByComponentCodeAsync(componentCode);
            if (existing == null)
            {
                return UnprocessableEntity("Garment component is not available for code :" + componentCode);
            }
            await _garmentComponentService.DeleteGarmentComponentAsync(componentCode);
            return NoContent();
        }
    }
}
