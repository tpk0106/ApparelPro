using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemFeatureService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/order-item-feature")]
    [ApiController]
    public class OrderItemFeatureController : ControllerBase
    {
        private readonly IOrderItemFeatureService _orderItemFeatureService;
        private readonly IMapper _mapper;

        public OrderItemFeatureController(IOrderItemFeatureService orderItemFeatureService, IMapper mapper)
        {
            _orderItemFeatureService = orderItemFeatureService ?? throw new ArgumentNullException(nameof(orderItemFeatureService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("list")]
        [Authorize(Policy = "order-item-feature-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<OrderItemFeatureMappingAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetOrderItemFeaturesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var orderItemFeatureServiceModels = await _orderItemFeatureService.GetOrderItemFeaturesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var orderItemFeatures = _mapper.Map<PaginationAPIModel<OrderItemFeatureMappingAPIModel>>(orderItemFeatureServiceModels);
            return Ok(orderItemFeatures);
        }

        [HttpGet("list/{stockCode}/{itemCode}", Name = "GetOrderItemFeatureByCodesAsync")]
        [Authorize(Policy = "order-item-feature-view")]
        [ProducesResponseType(typeof(OrderItemFeatureMappingAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetOrderItemFeatureByCodesAsync(string stockCode, string itemCode)
        {
            var orderItemFeature = await _orderItemFeatureService.GetOrderItemFeatureAsync(stockCode, itemCode);
            if (orderItemFeature == null)
            {
                return UnprocessableEntity("Order Item Feature mapping is not available for Stock Code : " + stockCode + " and Item Code : " + itemCode);
            }
            var orderItemFeatureAPIModel = _mapper.Map<OrderItemFeatureMappingAPIModel>(orderItemFeature);
            return Ok(orderItemFeatureAPIModel);
        }

        [HttpGet("list/does-exist/{stockCode}/{itemCode}", Name = "DoesOrderItemFeatureExistAsync")]
        [Authorize(Policy = "order-item-feature-view")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DoesOrderItemFeatureExistAsync(string stockCode, string itemCode)
        {
            var exists = await _orderItemFeatureService.DoesOrderItemFeatureExistAsync(stockCode, itemCode);
            return Ok(exists);
        }

        [HttpPost]
        [Authorize(Policy = "order-item-feature-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> AddOrderItemFeatureAsync([FromBody] CreateOrderItemFeatureMappingAPIModel createOrderItemFeatureMappingAPIModel)
        {
            try
            {
                var createOrderItemFeatureMappingServiceModel = _mapper.Map<CreateOrderItemFeatureMappingServiceModel>(createOrderItemFeatureMappingAPIModel);
                var addedOrderItemFeature = await _orderItemFeatureService.AddOrderItemFeatureAsync(createOrderItemFeatureMappingServiceModel);
                return CreatedAtRoute(nameof(GetOrderItemFeatureByCodesAsync),
                    new { stockCode = addedOrderItemFeature.StockCode, itemCode = addedOrderItemFeature.ItemCode }, null);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "order-item-feature-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteOrderItemFeatureAsync([FromQuery] string stockCode, [FromQuery] string itemCode)
        {
            var orderItemFeature = await _orderItemFeatureService.GetOrderItemFeatureAsync(stockCode, itemCode);
            if (orderItemFeature == null)
            {
                return UnprocessableEntity("Order Item Feature mapping is not available for Stock Code : " + stockCode + " and Item Code : " + itemCode);
            }
            await _orderItemFeatureService.DeleteOrderItemFeatureAsync(stockCode, itemCode);
            return NoContent();
        }

        [HttpPut]
        [Authorize(Policy = "order-item-feature-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> UpdateOrderItemFeatureAsync([FromQuery] string stockCode, [FromQuery] string itemCode,
            [FromBody] UpdateOrderItemFeatureMappingAPIModel updateOrderItemFeatureMappingAPIModel)
        {
            var existing = await _orderItemFeatureService.GetOrderItemFeatureAsync(stockCode, itemCode);
            if (existing == null)
            {
                return UnprocessableEntity("Order Item Feature mapping is not available for Stock Code : " + stockCode + " and Item Code : " + itemCode);
            }

            updateOrderItemFeatureMappingAPIModel.StockCode = stockCode;
            updateOrderItemFeatureMappingAPIModel.ItemCode = itemCode;

            try
            {
                var updateOrderItemFeatureMappingServiceModel = _mapper.Map<UpdateOrderItemFeatureMappingServiceModel>(updateOrderItemFeatureMappingAPIModel);
                await _orderItemFeatureService.UpdateOrderItemFeatureAsync(updateOrderItemFeatureMappingServiceModel);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
