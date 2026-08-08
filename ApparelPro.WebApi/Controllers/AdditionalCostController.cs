using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IAdditionalCostService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/additional-cost")]
    [ApiController]
    public class AdditionalCostController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAdditionalCostService _additionalCostService;

        public AdditionalCostController(IMapper mapper, IAdditionalCostService additionalCostService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _additionalCostService = additionalCostService ?? throw new ArgumentNullException(nameof(additionalCostService));
        }

        [HttpGet("list")]
        [Authorize(Policy = "additional-cost-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<AdditionalCostAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAdditionalCostsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var additionalCostServiceModels = await _additionalCostService.GetAdditionalCostsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var additionalCosts = _mapper.Map<PaginationAPIModel<AdditionalCostAPIModel>>(additionalCostServiceModels);
            return Ok(additionalCosts);
        }

        [HttpGet("list/{code}", Name = "GetAdditionalCostByCodeAsync")]
        [Authorize(Policy = "additional-cost-view")]
        [ProducesResponseType(typeof(AdditionalCostAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAdditionalCostByCodeAsync(string code)
        {
            var additionalCost = await _additionalCostService.GetAdditionalCostByCodeAsync(code);
            if (additionalCost == null)
            {
                return UnprocessableEntity("Additional Cost is not available for code : " + code);
            }
            var additionalCostAPIModel = _mapper.Map<AdditionalCostAPIModel>(additionalCost);
            return Ok(additionalCostAPIModel);
        }

        [HttpGet("list/does-exist/{code}", Name = "DoesAdditionalCostExistAsync")]
        [Authorize(Policy = "additional-cost-view")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DoesAdditionalCostExistAsync(string code)
        {
            var exists = await _additionalCostService.DoesAdditionalCostExistAsync(code);
            return Ok(exists);
        }

        [HttpPost]
        [Authorize(Policy = "additional-cost-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> AddAdditionalCostAsync([FromBody] CreateAdditionalCostAPIModel createAdditionalCostAPIModel)
        {
            try
            {
                var createAdditionalCostServiceModel = _mapper.Map<CreateAdditionalCostServiceModel>(createAdditionalCostAPIModel);
                var addedAdditionalCost = await _additionalCostService.AddAdditionalCostAsync(createAdditionalCostServiceModel);
                return CreatedAtRoute(nameof(GetAdditionalCostByCodeAsync), new { code = addedAdditionalCost.Code }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "additional-cost-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteAdditionalCostAsync([FromQuery] string code)
        {
            var additionalCost = await _additionalCostService.GetAdditionalCostByCodeAsync(code);
            if (additionalCost == null)
            {
                return UnprocessableEntity("Additional Cost is not available for code : " + code);
            }
            await _additionalCostService.DeleteAdditionalCostAsync(code);
            return NoContent();
        }

        [HttpPut]
        [Authorize(Policy = "additional-cost-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateAdditionalCostAsync([FromQuery] string code, [FromBody] UpdateAdditionalCostAPIModel updateAdditionalCostAPIModel)
        {
            var existing = await _additionalCostService.GetAdditionalCostByCodeAsync(code);
            if (existing == null)
            {
                return UnprocessableEntity("Additional Cost is not available for code : " + code);
            }
            updateAdditionalCostAPIModel.Code = code;
            var updateAdditionalCostServiceModel = _mapper.Map<UpdateAdditionalCostServiceModel>(updateAdditionalCostAPIModel);
            await _additionalCostService.UpdateAdditionalCostAsync(updateAdditionalCostServiceModel);
            return NoContent();
        }
    }
}
