using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IOperationService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/operation")]
    [ApiController]
    public class OperationController : ControllerBase
    {
        private readonly IOperationService _operationService;
        private readonly IMapper _mapper;

        public OperationController(IOperationService operationService, IMapper mapper)
        {
            _operationService = operationService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "operation-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<OperationAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetOperationsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _operationService.GetOperationsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var itemsPage = _mapper.Map<PaginationAPIModel<OperationAPIModel>>(serviceModels);
            return Ok(itemsPage);
        }

        [HttpGet("list/{operationCode}", Name = "GetOperationByOperationCodeAsync")]
        [Authorize(Policy = "operation-view")]
        [ProducesResponseType(typeof(OperationAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetOperationByOperationCodeAsync(string operationCode)
        {
            var operation = await _operationService.GetOperationByOperationCodeAsync(operationCode);
            if (operation == null)
            {
                return UnprocessableEntity("Operation is not available for code :" + operationCode);
            }
            return Ok(_mapper.Map<OperationAPIModel>(operation));
        }

        [HttpPost]
        [Authorize(Policy = "operation-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddOperationAsync([FromBody] CreateOperationAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateOperationServiceModel>(createAPIModel);
            var added = await _operationService.AddOperationAsync(createServiceModel);
            return CreatedAtRoute(nameof(GetOperationByOperationCodeAsync), new { operationCode = added.OperationCode }, null);
        }

        [HttpPut]
        [Authorize(Policy = "operation-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateOperationAsync([FromQuery] string operationCode, [FromBody] UpdateOperationAPIModel updateAPIModel)
        {
            var existing = await _operationService.GetOperationByOperationCodeAsync(operationCode);
            if (existing == null)
            {
                return UnprocessableEntity("Operation is not available for code :" + operationCode);
            }
            updateAPIModel.OperationCode = operationCode;
            var updateServiceModel = _mapper.Map<UpdateOperationServiceModel>(updateAPIModel);
            await _operationService.UpdateOperationAsync(updateServiceModel);
            return NoContent();
        }

        [HttpDelete("{operationCode}")]
        [Authorize(Policy = "operation-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteOperationAsync(string operationCode)
        {
            var existing = await _operationService.GetOperationByOperationCodeAsync(operationCode);
            if (existing == null)
            {
                return UnprocessableEntity("Operation is not available for code :" + operationCode);
            }
            await _operationService.DeleteOperationAsync(operationCode);
            return NoContent();
        }
    }
}
