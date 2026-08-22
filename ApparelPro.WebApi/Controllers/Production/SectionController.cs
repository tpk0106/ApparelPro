using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.ISectionService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/section")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;
        private readonly IMapper _mapper;

        public SectionController(ISectionService sectionService, IMapper mapper)
        {
            _sectionService = sectionService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "section-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<SectionAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSectionsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _sectionService.GetSectionsAsync(
                pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            return Ok(_mapper.Map<PaginationAPIModel<SectionAPIModel>>(serviceModels));
        }

        [HttpGet("list/all")]
        [Authorize(Policy = "section-view")]
        [ProducesResponseType(typeof(List<SectionAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAllSectionsAsync()
        {
            var serviceModels = await _sectionService.GetAllSectionsAsync();
            return Ok(_mapper.Map<List<SectionAPIModel>>(serviceModels));
        }

        [HttpPost]
        [Authorize(Policy = "section-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddSectionAsync([FromBody] CreateSectionAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateSectionServiceModel>(createAPIModel);
            await _sectionService.AddSectionAsync(createServiceModel);
            return StatusCode(HttpStatusCodes.Created);
        }

        [HttpPut]
        [Authorize(Policy = "section-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateSectionAsync([FromBody] UpdateSectionAPIModel updateAPIModel)
        {
            var updateServiceModel = _mapper.Map<UpdateSectionServiceModel>(updateAPIModel);
            await _sectionService.UpdateSectionAsync(updateServiceModel);
            return NoContent();
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "section-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> DeleteSectionAsync(string code)
        {
            await _sectionService.DeleteSectionAsync(code);
            return NoContent();
        }
    }
}
