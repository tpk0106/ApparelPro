using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IComponentOperationTemplateService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/component-operation-template")]
    [ApiController]
    public class ComponentOperationTemplateController : ControllerBase
    {
        private readonly IComponentOperationTemplateService _componentOperationTemplateService;
        private readonly IMapper _mapper;

        public ComponentOperationTemplateController(
            IComponentOperationTemplateService componentOperationTemplateService, IMapper mapper)
        {
            _componentOperationTemplateService = componentOperationTemplateService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "component-operation-template-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<ComponentOperationTemplateAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetComponentOperationTemplatesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _componentOperationTemplateService.GetComponentOperationTemplatesAsync(
                pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            return Ok(_mapper.Map<PaginationAPIModel<ComponentOperationTemplateAPIModel>>(serviceModels));
        }

        [HttpGet("by-component/{componentCode}")]
        [Authorize(Policy = "component-operation-template-view")]
        [ProducesResponseType(typeof(List<ComponentOperationTemplateAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetTemplatesByComponentAsync(string componentCode)
        {
            var serviceModels = await _componentOperationTemplateService.GetTemplatesByComponentAsync(componentCode);
            return Ok(_mapper.Map<List<ComponentOperationTemplateAPIModel>>(serviceModels));
        }

        [HttpPost]
        [Authorize(Policy = "component-operation-template-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddComponentOperationTemplateAsync([FromBody] CreateComponentOperationTemplateAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateComponentOperationTemplateServiceModel>(createAPIModel);
            var added = await _componentOperationTemplateService.AddComponentOperationTemplateAsync(createServiceModel);
            return CreatedAtRoute(nameof(GetTemplatesByComponentAsync), new { componentCode = added.ComponentCode }, null);
        }

        [HttpPut]
        [Authorize(Policy = "component-operation-template-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateComponentOperationTemplateAsync([FromBody] UpdateComponentOperationTemplateAPIModel updateAPIModel)
        {
            var updateServiceModel = _mapper.Map<UpdateComponentOperationTemplateServiceModel>(updateAPIModel);
            await _componentOperationTemplateService.UpdateComponentOperationTemplateAsync(updateServiceModel);
            return NoContent();
        }

        [HttpDelete("{componentCode}/{operationSequence}")]
        [Authorize(Policy = "component-operation-template-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> DeleteComponentOperationTemplateAsync(string componentCode, int operationSequence)
        {
            await _componentOperationTemplateService.DeleteComponentOperationTemplateAsync(componentCode, operationSequence);
            return NoContent();
        }
    }
}
