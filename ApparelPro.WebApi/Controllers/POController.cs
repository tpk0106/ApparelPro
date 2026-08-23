using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.OrderManagement;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/po")]
    [ApiController]
   // [Authorize("RegisteredUser")]
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

        [HttpGet("list/buyer", Name = "GetOrdersByBuyerCodeAsync")]
        [ProducesResponseType(typeof(List<string>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetPurchaseOrderByBuyerCodeAsync([FromQuery] int buyerCode)
        {
            var poList = await _purchaseOrderService.GetPurchaseOrderByBuyerCodeAsync(buyerCode);
            if (poList == null)
            {
                return UnprocessableEntity("POs not available for buyer :" + buyerCode);
            }            
            return Ok(poList);
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

        //[HttpGet("list/type/order", Name = "GetPurchaseOrderByBuyerAndOrderAsync")]
        //[ProducesResponseType(typeof(POAPIModel), HttpStatusCodes.OK)]
        //[ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        //public async Task<IActionResult> GetPurchaseOrderByBuyerAndOrderAsync([FromQuery] int buyer, [FromQuery] string order)
        //{
        //    var po = await _purchaseOrderService.GetPurchaseOrderByBuyerAndOrderAsync(buyer, order);
        //    if (po == null)
        //    {
        //        return UnprocessableEntity("PO is not available for buyer/Order :" + buyer);
        //    }
        //    var poAPIModel = _mapper.Map<POAPIModel>(po);
        //    return Ok(poAPIModel);
        //}

        [HttpPost()]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(CreatedResult), HttpStatusCodes.Created)]
        public async Task<IActionResult> AddPurchaseOrderAsync([FromBody] CreatePOAPIModel createPOAPIModel)
        {
            var createPOServiceModel = _mapper.Map<CreatePurchaseOrderServiceModel>(createPOAPIModel);
            var addedPO = await _purchaseOrderService.AddPurchaseOrderAsync(createPOServiceModel);
            return CreatedAtRoute(nameof(GetPurchaseOrderByBuyerAndOrderAsync),
                new
                {
                    addedPO.BuyerCode,
                    addedPO.Order,                  
                }, null);
        }

        [HttpPost("save-supplier-po")]
        [ProducesResponseType(typeof(CreatedResult), HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)] // Add failure response type
        public async Task<IActionResult> SaveSupplierPurchaseOrderAsync(
              string purchaseNumber, string supplierCode, string storeCode,
             string proformaNo, DateOnly? proformaDate, string currencyCode,
             List<SupplierPurchaseOrderDetails> lineItems
        )
        {
            // 1. Get the boolean result from your service
            bool isSaved = await _purchaseOrderService.SaveSupplierPurchaseOrderAsync(
                purchaseNumber, supplierCode, storeCode, proformaNo, proformaDate, currencyCode, lineItems);

            // 2. Evaluate the boolean and return the correct HTTP wrapper
            if (!isSaved)
            {
                return BadRequest("Failed to save the supplier purchase order.");
            }

            // Wrap the response in a CreatedResult to match your [ProducesResponseType] attribute
            return Created(string.Empty, new { Message = "Purchase order saved successfully." });
        }

    }
}

