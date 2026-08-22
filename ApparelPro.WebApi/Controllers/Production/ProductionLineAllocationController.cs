using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/production-line-allocation")]
    [ApiController]
    public class ProductionLineAllocationController : ControllerBase
    {
        private readonly IProductionLineAllocationService _productionLineAllocationService;
        private readonly IMapper _mapper;

        public ProductionLineAllocationController(
            IProductionLineAllocationService productionLineAllocationService, IMapper mapper)
        {
            _productionLineAllocationService = productionLineAllocationService;
            _mapper = mapper;
        }

        [HttpGet("by-shipment")]
        [Authorize(Policy = "production-line-allocation-view")]
        [ProducesResponseType(typeof(List<ProductionLineAllocationAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetByShipmentAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode,
            [FromQuery] string styleCode, [FromQuery] string shipmentOrder)
        {
            var serviceModels = await _productionLineAllocationService.GetByShipmentAsync(
                buyerCode, order, typeCode, styleCode, shipmentOrder);
            return Ok(_mapper.Map<List<ProductionLineAllocationAPIModel>>(serviceModels));
        }

        [HttpGet("by-line")]
        [Authorize(Policy = "production-line-allocation-view")]
        [ProducesResponseType(typeof(List<ProductionLineAllocationAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetByLineAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode,
            [FromQuery] string styleCode, [FromQuery] string lineCode)
        {
            var serviceModels = await _productionLineAllocationService.GetByLineAsync(
                buyerCode, order, typeCode, styleCode, lineCode);
            return Ok(_mapper.Map<List<ProductionLineAllocationAPIModel>>(serviceModels));
        }

        [HttpPost("manual")]
        [Authorize(Policy = "production-line-allocation-manage")]
        [ProducesResponseType(typeof(ProductionLineAllocationAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> ManualAllocateAsync([FromBody] ManualAllocateProductionLineAPIModel payload)
        {
            var serviceModel = _mapper.Map<ManualAllocateProductionLineServiceModel>(payload);
            try
            {
                var result = await _productionLineAllocationService.ManualAllocateAsync(serviceModel);
                return Ok(_mapper.Map<ProductionLineAllocationAPIModel>(result));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }

        [HttpPost("automatic")]
        [Authorize(Policy = "production-line-allocation-manage")]
        [ProducesResponseType(typeof(ProductionLineAllocationResultAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> AutomaticAllocateAsync([FromBody] AutomaticAllocateProductionLineAPIModel payload)
        {
            var serviceModel = _mapper.Map<AutomaticAllocateProductionLineServiceModel>(payload);
            var result = await _productionLineAllocationService.AutomaticAllocateAsync(serviceModel);
            return Ok(_mapper.Map<ProductionLineAllocationResultAPIModel>(result));
        }

        [HttpDelete]
        [Authorize(Policy = "production-line-allocation-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> DeleteAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode,
            [FromQuery] string styleCode, [FromQuery] string shipmentOrder, [FromQuery] string lineCode)
        {
            await _productionLineAllocationService.DeleteAsync(
                buyerCode, order, typeCode, styleCode, shipmentOrder, lineCode);
            return NoContent();
        }
    }
}
