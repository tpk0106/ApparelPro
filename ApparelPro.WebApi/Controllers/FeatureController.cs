using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApparelPro.WebApi.APIModels.OrderManagement;
using apparelPro.BusinessLogic.Services.Models.Reference.IFeatureService;
using apparelPro.BusinessLogic.Services;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/feature")]
    [ApiController]
    public class FeatureController : ControllerBase
    {
        private readonly IFeatureService _featureService;
        private readonly IMapper _mapper;
        public FeatureController(IFeatureService featureService, IMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (featureService == null)
            {
                throw new ArgumentNullException(nameof(featureService));
            }
            _featureService = featureService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        //  [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied        
        [ProducesResponseType(typeof(PaginationAPIModel<BuyerAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetFeaturesAsync(
         [FromQuery] int pageSize,
         [FromQuery] int pageNumber,
         [FromQuery] string? sortColumn = null,
         [FromQuery] string? sortOrder = null,
         [FromQuery] string? filterColumn = null,
         [FromQuery] string? filterQuery = null)
        {
            var featureServiceModels = await _featureService.GetFeaturesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var features = _mapper.Map<PaginationAPIModel<FeatureAPIModel>>(featureServiceModels);
            return Ok(features);
        }

        [HttpGet("list/{id}", Name = "GetFeatureByFeatureIdAsync")]
        [ProducesResponseType(typeof(FeatureAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetFeatureByFeatureIdAsync([FromRoute] int id)
        {
            var feature = await _featureService.GetFeatureByIdAsync(id);
            if (feature == null)
            {
                return UnprocessableEntity("feature is not available for feature id :" + id);
            }
            var featureAPIModel = _mapper.Map<FeatureAPIModel>(feature);
            return Ok(featureAPIModel);
        }

        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddFeatureAsync([FromBody] CreateFeatureAPIModel createFeatureAPIModel)
        {
            var createFeatureServiceModel = _mapper.Map<CreateFeatureServiceModel>(createFeatureAPIModel);
            var addedFeature = await _featureService.AddFeatureAsync(createFeatureServiceModel);
            return CreatedAtRoute(nameof(GetFeatureByFeatureIdAsync), new { id = addedFeature.Id }, null);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteFeatureAsync(int id)
        {
            var feature = await _featureService.GetFeatureByIdAsync(id);
            if (feature == null)
            {
                return UnprocessableEntity("feature is not available for id :" + id);
            }
            await _featureService.DeleteFeatureAsync(id);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]        
        public async Task<IActionResult> UpdateFeatureAsync([FromQuery] int id, [FromBody] UpdateFeatureAPIModel
            updateFeatureAPIModel)
        {
            var resultFeatureAPIModel = _mapper.Map<FeatureAPIModel>(await _featureService.GetFeatureByIdAsync(id));

            if (resultFeatureAPIModel == null)
            {
                return UnprocessableEntity("Feature is not available for id :" + id);
            }
            var updateFeatureServiceModel = _mapper.Map<UpdateFeatureServiceModel>(updateFeatureAPIModel);
            await _featureService.UpdateFeatureAsync(updateFeatureServiceModel);
            return NoContent();
        }
    }
}
