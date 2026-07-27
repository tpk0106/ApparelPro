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
    [Route("api/item-feature")]
    [ApiController]
    public class ItemFeatureController : ControllerBase
    {
        private readonly IItemFeatureService _itemFeatureService;
        private readonly IMapper _mapper;
        public ItemFeatureController(IItemFeatureService itemFeatureService, IMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (itemFeatureService == null)
            {
                throw new ArgumentNullException(nameof(itemFeatureService));
            }
            _itemFeatureService = itemFeatureService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        //  [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied        
        [ProducesResponseType(typeof(PaginationAPIModel<BuyerAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetItemFeaturesAsync(
         [FromQuery] int pageSize,
         [FromQuery] int pageNumber,
         [FromQuery] string? sortColumn = null,
         [FromQuery] string? sortOrder = null,
         [FromQuery] string? filterColumn = null,
         [FromQuery] string? filterQuery = null)
        {
            var featureServiceModels = await _itemFeatureService.GetItemFeaturesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var features = _mapper.Map<PaginationAPIModel<ItemFeatureAPIModel>>(featureServiceModels);
            return Ok(features);
        }

        [HttpGet("list/{featureCode}", Name = "GetItemFeatureByFeatureCodeAsync")]
        [ProducesResponseType(typeof(ItemFeatureAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetItemFeatureByFeatureCodeAsync([FromRoute] string featureCode)
        {
            var feature = await _itemFeatureService.GetItemFeatureByFeatureCodeAsync(featureCode);
            if (feature == null)
            {
                return UnprocessableEntity("feature is not available for feature id :" + featureCode);
            }
            var featureAPIModel = _mapper.Map<ItemFeatureAPIModel>(feature);
            return Ok(featureAPIModel);
        }

        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddFeatureAsync([FromBody] CreateItemFeatureAPIModel createItemFeatureAPIModel)
        {
            var createItemFeatureServiceModel = _mapper.Map<CreateItemFeatureServiceModel>(createItemFeatureAPIModel);
            var addedFeature = await _itemFeatureService.AddItemFeatureAsync(createItemFeatureServiceModel);
            return CreatedAtRoute(nameof(GetItemFeatureByFeatureCodeAsync), new { id = addedFeature.FeatureCode }, null);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteFeatureAsync(string featureCode)
        {
            var feature = await _itemFeatureService.GetItemFeatureByFeatureCodeAsync(featureCode);
            if (feature == null)
            {
                return UnprocessableEntity("feature is not available for id :" + featureCode);
            }
            await _itemFeatureService.DeleteItemFeatureAsync(featureCode);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]        
        public async Task<IActionResult> UpdateFeatureAsync([FromQuery] string featureCode, [FromBody] UpdateItemFeatureAPIModel
            updateItemFeatureAPIModel)
        {
            var resultFeatureAPIModel = _mapper.Map<ItemFeatureAPIModel>(await _itemFeatureService.GetItemFeatureByFeatureCodeAsync(featureCode));

            if (resultFeatureAPIModel == null)
            {
                return UnprocessableEntity("Feature is not available for id :" + featureCode);
            }
            var updateItemFeatureServiceModel = _mapper.Map<UpdateItemFeatureServiceModel>(updateItemFeatureAPIModel);
            await _itemFeatureService.UpdateItemFeatureAsync(updateItemFeatureServiceModel);
            return NoContent();
        }
    }
}
