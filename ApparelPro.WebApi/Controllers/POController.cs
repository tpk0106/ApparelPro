using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/po")]
    [ApiController]
    [Authorize("RegisteredUser")]
    public class POController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPurchaseOrderService _purchaseOrderService;
        public POController(IPurchaseOrderService purchaseOrderService, IMapper mapper)
        {
            if(purchaseOrderService == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrderService));
            }
            if(mapper == null)
            {
                throw new ArgumentNullException(nameof (mapper));
            }
            _mapper = mapper;
            _purchaseOrderService = purchaseOrderService;            
        }

        [HttpGet("list")]
        [Authorize("Merchandising")]
        [ProducesResponseType(typeof(PaginationAPIModel<POAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetPurchaseOrderAsync([FromQuery] int pageSize, [FromQuery] int pageNumber, 
            string? sortColumn = null, string? sortOrder = null, string? filterColumn = null, string? filterQuery = null)
        {
            var poServiceModels = await _purchaseOrderService.GetPurchaseOrderAsync(pageSize, pageNumber,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var pos = _mapper.Map<PaginationAPIModel<POAPIModel>>(poServiceModels);
            return Ok(pos);
        }

        [HttpGet("list/buyer/order", Name = "GetPurchaseOrderByBuyerAndOrderAsync")]
        [ProducesResponseType(typeof(POAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetPurchaseOrderByBuyerAndOrderAsync([FromQuery] int buyer,[FromQuery] string order)
        {
            var po = await _purchaseOrderService.GetPurchaseOrderByBuyerAndOrderAsync(buyer, order);
            if (po == null)
            {
                return UnprocessableEntity("PO is not available for buyer/Order :" + buyer);
            }
            var poAPIModel = _mapper.Map<POAPIModel>(po);
            return Ok(poAPIModel);
        }
    }
}
