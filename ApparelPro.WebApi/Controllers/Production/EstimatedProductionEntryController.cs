using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionEntryService;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/estimated-production-entry")]
    [ApiController]
    public class EstimatedProductionEntryController : ControllerBase
    {
        private readonly IEstimatedProductionEntryService _estimatedProductionEntryService;
        private readonly IMapper _mapper;

        public EstimatedProductionEntryController(
            IEstimatedProductionEntryService estimatedProductionEntryService, IMapper mapper)
        {
            _estimatedProductionEntryService = estimatedProductionEntryService;
            _mapper = mapper;
        }

        [HttpGet("by-line")]
        [Authorize(Policy = "estimated-production-entry-view")]
        [ProducesResponseType(typeof(List<EstimatedProductionEntryAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetByLineAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode, [FromQuery] string lineCode)
        {
            var serviceModels = await _estimatedProductionEntryService.GetByLineAsync(
                buyerCode, order, typeCode, styleCode, lineCode);
            return Ok(_mapper.Map<List<EstimatedProductionEntryAPIModel>>(serviceModels));
        }

        [HttpPost("bulk-save")]
        [Authorize(Policy = "estimated-production-entry-manage")]
        [ProducesResponseType(typeof(List<EstimatedProductionEntryAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> BulkSaveAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode, [FromQuery] string lineCode,
            [FromBody] List<CreateEstimatedProductionEntryAPIModel> payload)
        {
            if (payload == null) return BadRequest("Estimated production entry payload cannot be empty.");

            var serviceModels = _mapper.Map<List<CreateEstimatedProductionEntryServiceModel>>(payload);

            try
            {
                var result = await _estimatedProductionEntryService.BulkSaveAsync(
                    buyerCode, order, typeCode, styleCode, lineCode, serviceModels);
                return Ok(_mapper.Map<List<EstimatedProductionEntryAPIModel>>(result));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
