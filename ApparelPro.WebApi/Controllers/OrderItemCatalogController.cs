using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemCatalogService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    // Order Items Catalog (od_itm) - Stock/Item master list. Legacy enters this via
    // [F1] help (Stock via F1, Item Code typed and validated) e.g. in OD_AITM1.PRG;
    // this controller is the missing "maintain the catalog itself" screen for the
    // same OrderItems table the Garment Type Item Requirements and Additional Cost
    // per Garment pickers already read from.
    [Route("api/order-item-catalog")]
    [ApiController]
    public class OrderItemCatalogController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IOrderItemCatalogService _orderItemCatalogService;

        public OrderItemCatalogController(IMapper mapper, IOrderItemCatalogService orderItemCatalogService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _orderItemCatalogService = orderItemCatalogService ?? throw new ArgumentNullException(nameof(orderItemCatalogService));
        }

        [HttpGet("list")]
        [Authorize(Policy = "order-item-catalog-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<OrderItemCatalogAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetOrderItemCatalogAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var orderItemCatalogServiceModels = await _orderItemCatalogService.GetOrderItemCatalogAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var orderItemCatalog = _mapper.Map<PaginationAPIModel<OrderItemCatalogAPIModel>>(orderItemCatalogServiceModels);
            return Ok(orderItemCatalog);
        }

        [HttpGet("list/{stockCode}/{itemCode}", Name = "GetOrderItemCatalogByCodeAsync")]
        [Authorize(Policy = "order-item-catalog-view")]
        [ProducesResponseType(typeof(OrderItemCatalogAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetOrderItemCatalogByCodeAsync(string stockCode, string itemCode)
        {
            var orderItemCatalog = await _orderItemCatalogService.GetOrderItemCatalogByCodeAsync(stockCode, itemCode);
            if (orderItemCatalog == null)
            {
                return UnprocessableEntity("Order Item Catalog entry is not available for Stock/Item : " + stockCode + "/" + itemCode);
            }
            var orderItemCatalogAPIModel = _mapper.Map<OrderItemCatalogAPIModel>(orderItemCatalog);
            return Ok(orderItemCatalogAPIModel);
        }

        [HttpGet("list/does-exist/{stockCode}/{itemCode}", Name = "DoesOrderItemCatalogExistAsync")]
        [Authorize(Policy = "order-item-catalog-view")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DoesOrderItemCatalogExistAsync(string stockCode, string itemCode)
        {
            var exists = await _orderItemCatalogService.DoesOrderItemCatalogExistAsync(stockCode, itemCode);
            return Ok(exists);
        }

        [HttpPost]
        [Authorize(Policy = "order-item-catalog-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> AddOrderItemCatalogAsync([FromBody] CreateOrderItemCatalogAPIModel createOrderItemCatalogAPIModel)
        {
            try
            {
                var createOrderItemCatalogServiceModel = _mapper.Map<CreateOrderItemCatalogServiceModel>(createOrderItemCatalogAPIModel);
                var added = await _orderItemCatalogService.AddOrderItemCatalogAsync(createOrderItemCatalogServiceModel);
                return CreatedAtRoute(nameof(GetOrderItemCatalogByCodeAsync), new { stockCode = added.StockCode, itemCode = added.ItemCode }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "order-item-catalog-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteOrderItemCatalogAsync([FromQuery] string stockCode, [FromQuery] string itemCode)
        {
            var existing = await _orderItemCatalogService.GetOrderItemCatalogByCodeAsync(stockCode, itemCode);
            if (existing == null)
            {
                return UnprocessableEntity("Order Item Catalog entry is not available for Stock/Item : " + stockCode + "/" + itemCode);
            }
            try
            {
                await _orderItemCatalogService.DeleteOrderItemCatalogAsync(stockCode, itemCode);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return NoContent();
        }

        [HttpPut]
        [Authorize(Policy = "order-item-catalog-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateOrderItemCatalogAsync(
            [FromQuery] string stockCode, [FromQuery] string itemCode, [FromBody] UpdateOrderItemCatalogAPIModel updateOrderItemCatalogAPIModel)
        {
            var existing = await _orderItemCatalogService.GetOrderItemCatalogByCodeAsync(stockCode, itemCode);
            if (existing == null)
            {
                return UnprocessableEntity("Order Item Catalog entry is not available for Stock/Item : " + stockCode + "/" + itemCode);
            }
            updateOrderItemCatalogAPIModel.StockCode = stockCode;
            updateOrderItemCatalogAPIModel.ItemCode = itemCode;
            var updateOrderItemCatalogServiceModel = _mapper.Map<UpdateOrderItemCatalogServiceModel>(updateOrderItemCatalogAPIModel);
            await _orderItemCatalogService.UpdateOrderItemCatalogAsync(updateOrderItemCatalogServiceModel);
            return NoContent();
        }
    }
}
