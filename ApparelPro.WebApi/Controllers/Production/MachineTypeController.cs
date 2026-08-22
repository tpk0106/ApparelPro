using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IMachineTypeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/machine-type")]
    [ApiController]
    public class MachineTypeController : ControllerBase
    {
        private readonly IMachineTypeService _machineTypeService;
        private readonly IMapper _mapper;

        public MachineTypeController(IMachineTypeService machineTypeService, IMapper mapper)
        {
            _machineTypeService = machineTypeService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "machine-type-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<MachineTypeAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetMachineTypesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _machineTypeService.GetMachineTypesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var itemsPage = _mapper.Map<PaginationAPIModel<MachineTypeAPIModel>>(serviceModels);
            return Ok(itemsPage);
        }

        [HttpGet("list/{code}", Name = "GetMachineTypeByCodeAsync")]
        [Authorize(Policy = "machine-type-view")]
        [ProducesResponseType(typeof(MachineTypeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetMachineTypeByCodeAsync(string code)
        {
            var machineType = await _machineTypeService.GetMachineTypeByCodeAsync(code);
            if (machineType == null)
            {
                return UnprocessableEntity("Machine type is not available for code :" + code);
            }
            return Ok(_mapper.Map<MachineTypeAPIModel>(machineType));
        }

        [HttpPost]
        [Authorize(Policy = "machine-type-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddMachineTypeAsync([FromBody] CreateMachineTypeAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateMachineTypeServiceModel>(createAPIModel);
            var added = await _machineTypeService.AddMachineTypeAsync(createServiceModel);
            return CreatedAtRoute(nameof(GetMachineTypeByCodeAsync), new { code = added.Code }, null);
        }

        [HttpPut]
        [Authorize(Policy = "machine-type-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateMachineTypeAsync([FromQuery] string code, [FromBody] UpdateMachineTypeAPIModel updateAPIModel)
        {
            var existing = await _machineTypeService.GetMachineTypeByCodeAsync(code);
            if (existing == null)
            {
                return UnprocessableEntity("Machine type is not available for code :" + code);
            }
            updateAPIModel.Code = code;
            var updateServiceModel = _mapper.Map<UpdateMachineTypeServiceModel>(updateAPIModel);
            await _machineTypeService.UpdateMachineTypeAsync(updateServiceModel);
            return NoContent();
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "machine-type-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteMachineTypeAsync(string code)
        {
            var existing = await _machineTypeService.GetMachineTypeByCodeAsync(code);
            if (existing == null)
            {
                return UnprocessableEntity("Machine type is not available for code :" + code);
            }
            await _machineTypeService.DeleteMachineTypeAsync(code);
            return NoContent();
        }
    }
}
