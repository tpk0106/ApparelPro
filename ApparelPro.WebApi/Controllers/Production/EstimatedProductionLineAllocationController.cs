using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionLineAllocationService;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/estimated-production-line-allocation")]
    [ApiController]
    public class EstimatedProductionLineAllocationController : ControllerBase
    {
        private readonly IEstimatedProductionLineAllocationService _estimatedProductionLineAllocationService;
        private readonly IMapper _mapper;

        public EstimatedProductionLineAllocationController(
            IEstimatedProductionLineAllocationService estimatedProductionLineAllocationService, IMapper mapper)
        {
            _estimatedProductionLineAllocationService = estimatedProductionLineAllocationService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "estimated-production-line-allocation-view")]
        [ProducesResponseType(typeof(EstimatedProductionLineAllocationAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync([FromQuery] int buyerCode, [FromQuery] string styleCode)
        {
            var serviceModel = await _estimatedProductionLineAllocationService.GetAsync(buyerCode, styleCode);
            if (serviceModel == null)
            {
                return UnprocessableEntity("No estimated production line allocation exists for this buyer/style.");
            }
            return Ok(_mapper.Map<EstimatedProductionLineAllocationAPIModel>(serviceModel));
        }

        [HttpPost("manual")]
        [Authorize(Policy = "estimated-production-line-allocation-manage")]
        [ProducesResponseType(typeof(EstimatedProductionLineAllocationAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> ManualAllocateAsync(
            [FromBody] ManualAllocateEstimatedProductionLineAPIModel payload)
        {
            var serviceModel = _mapper.Map<ManualAllocateEstimatedProductionLineServiceModel>(payload);
            var result = await _estimatedProductionLineAllocationService.ManualAllocateAsync(serviceModel);
            return Ok(_mapper.Map<EstimatedProductionLineAllocationAPIModel>(result));
        }

        [HttpPost("automatic")]
        [Authorize(Policy = "estimated-production-line-allocation-manage")]
        [ProducesResponseType(typeof(EstimatedProductionLineAllocationResultAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> AutomaticAllocateAsync(
            [FromBody] AutomaticAllocateEstimatedProductionLineAPIModel payload)
        {
            var serviceModel = _mapper.Map<AutomaticAllocateEstimatedProductionLineServiceModel>(payload);
            var result = await _estimatedProductionLineAllocationService.AutomaticAllocateAsync(serviceModel);
            return Ok(_mapper.Map<EstimatedProductionLineAllocationResultAPIModel>(result));
        }

        [HttpDelete]
        [Authorize(Policy = "estimated-production-line-allocation-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> DeleteAsync([FromQuery] int buyerCode, [FromQuery] string styleCode)
        {
            await _estimatedProductionLineAllocationService.DeleteAsync(buyerCode, styleCode);
            return NoContent();
        }
    }
}
