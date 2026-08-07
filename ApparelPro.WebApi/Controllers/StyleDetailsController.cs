using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.Reference;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using ApparelPro.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/styleDetails")]
    [ApiController]
    public class StyleDetailsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IStyleDetailsService _styleDetailsService;
        public StyleDetailsController(IMapper mapper, IStyleDetailsService styleDetailsService)
        {
            _mapper = mapper;
            _styleDetailsService = styleDetailsService;            
        }

        [HttpGet("list")]
        [Authorize(Policy = "style-details")]
        //[Authorize("Merchandising")] // policy applied
        //[Authorize(Roles = "Inventory")]
        // [Authorize("RegisteredUser")]
        [ProducesResponseType(typeof(PaginationAPIModel<StyleAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetStyleDetailsAsync(
          [FromQuery] int pageSize,
          [FromQuery] int pageNumber,
          [FromQuery] string? sortColumn = null,
          [FromQuery] string? sortOrder = null,
          [FromQuery] string? filterColumn = null,
          [FromQuery] string? filterQuery = null)
        {
            var styleServiceModels = await _styleDetailsService.GetStyleDetailsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var styles = _mapper.Map<PaginationAPIModel<StyleAPIModel>>(styleServiceModels);
            return Ok(styles);
        }

        [HttpPost]
        [Authorize(Policy = "style-details")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> AddStyleDetailsAsync([FromBody] CreateStyleDetailsAPIModel createStyleDetailsAPIModel)
        {
            try
            {
                // ClaimTypes.Name carries the authenticated user's email (see SecurityService.
                // GetClaimsAsync) - the only identity claim issued, used here purely for the
                // quantity-override audit trail. Null if the request is unauthenticated.
                var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                var createStyleDetailsServiceModel = _mapper.Map<CreateStyleDetailsServiceModel>(createStyleDetailsAPIModel);
                var addedStyle = await _styleDetailsService.AddStyleDetailsAsync(createStyleDetailsServiceModel, currentUserEmail);
                return CreatedAtRoute(nameof(GetStyleDetailsByBuyerOrderTypeStyleAsync),
                    new
                    {
                        buyer = addedStyle.BuyerCode,
                        order = addedStyle.Order,
                        type = addedStyle.TypeCode,
                        style = addedStyle.StyleCode
                    }, null);
            }
            catch (InvalidOperationException ex)
            {
                // Total Quantity exceeded, or the entered Unit has no conversion path to the
                // order's unit - see StyleDetailsService.ValidateStyleQuantityAsync.
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut()]
        [Authorize(Policy = "style-details")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> UpdateStyleDetailsAsync([FromBody] UpdateStyleDetailsAPIModel updateStyleDetailsAPIModel)
        {
            int buyerCode = updateStyleDetailsAPIModel.BuyerCode;
            string order = updateStyleDetailsAPIModel.Order;
            int type = updateStyleDetailsAPIModel.TypeCode;
            string style = updateStyleDetailsAPIModel.StyleCode;

            var resultStyleAPIModel = _mapper.Map<StyleAPIModel>(
                await _styleDetailsService.GetStyleDetailsByBuyerOrderTypeStyleAsync(buyerCode, order, type,style));

            if (resultStyleAPIModel == null)
            {
                return UnprocessableEntity("Style is not available for code :" + style);
            }
            resultStyleAPIModel.Quantity =updateStyleDetailsAPIModel.Quantity;
            resultStyleAPIModel.UnitPrice =updateStyleDetailsAPIModel.UnitPrice;
            resultStyleAPIModel.Unit = updateStyleDetailsAPIModel.Unit;

            var updateStyleDetailsServiceModel = _mapper.Map<UpdateStyleDetailsServiceModel>(resultStyleAPIModel);

            try
            {
                var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                await _styleDetailsService.UpdateStyleDetailsAsync(updateStyleDetailsServiceModel, currentUserEmail);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return NoContent();
        }

        // Live running total for the Order Confirmation Styles grid header - sum of every
        // style's quantity (converted into the order's unit) plus the order's own Total
        // Quantity, so the frontend can display both and flag an overage without needing to
        // page through every style row itself (the list endpoint above is paginated).
        [HttpGet("list/buyer/order/totals", Name = "GetStyleTotalsAsync")]
        [ProducesResponseType(typeof(StyleTotalsAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetStyleTotalsAsync([FromQuery] int buyerCode, [FromQuery] string order)
        {
            var totals = await _styleDetailsService.GetStyleTotalsAsync(buyerCode, order);
            return Ok(_mapper.Map<StyleTotalsAPIModel>(totals));
        }

        [HttpGet("list/buyer/order", Name = "GetStyleDetailsByBuyerAndOrderAsync")]
        [ProducesResponseType(typeof(PaginationAPIModel<StyleAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetStyleDetailsByBuyerAndOrderAsync(int buyerCode, string order,
          [FromQuery] int pageSize,
          [FromQuery] int pageNumber,
          [FromQuery] string? sortColumn = null,
          [FromQuery] string? sortOrder = null,
          [FromQuery] string? filterColumn = null,
          [FromQuery] string? filterQuery = null
        )
        {
            var styleServiceModels = await _styleDetailsService.GetStyleDetailsByBuyerOrderAsync(buyerCode, order, 
                pageNumber,pageSize,sortColumn, sortOrder, filterColumn, filterQuery);           
            var styleAPIModels = _mapper.Map<PaginationAPIModel<StyleAPIModel>>(styleServiceModels);
            return Ok(styleAPIModels);
        }

        [HttpGet("list/{buyer}/{order}/{type}/{style}", Name = "GetStyleDetailsByBuyerOrderTypeStyleAsync")]
        [ProducesResponseType(typeof(StyleAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetStyleDetailsByBuyerOrderTypeStyleAsync(
            [FromRoute]  int buyer,
            [FromRoute] string order,
            [FromRoute] int type, 
            [FromRoute] string style)
        {
            // FIXED (2026-08-07): this whole block was a Bank/Country-controller copy-paste
            // artifact (variable named "Bank", error message "Bank is not available", mapped to
            // CountryAPIModel) never updated for Style - found via a full _mapper.Map<> sweep,
            // since it threw AutoMapperMappingException the moment this route was ever hit
            // (no CreateMap<StyleDetailsServiceModel, CountryAPIModel> exists, nor should one).
            // UpdateStyleDetailsAsync below already maps this exact same service call
            // (GetStyleDetailsByBuyerOrderTypeStyleAsync) to StyleAPIModel - that's the
            // established, already-registered pair (CreateMap<StyleDetailsServiceModel,
            // StyleAPIModel> in ServicetoAPIModelMappings.cs), so it's what this route should
            // have returned all along.
            var styleDetails = await _styleDetailsService.GetStyleDetailsByBuyerOrderTypeStyleAsync(buyer,order,type,style);
            if (styleDetails == null)
            {
                return UnprocessableEntity("Style is not available for Buyer/Order/Type/Style :" + buyer + "/" + order + "/" + type + "/" + style);
            }
            var styleAPIModel = _mapper.Map<StyleAPIModel>(styleDetails);
            return Ok(styleAPIModel);
        }

        [HttpGet("list/styles/{buyerCode}/{order}/{typeCode}", Name = "GetStyleDetailsByBuyerOrderTypeAsync")]
        [ProducesResponseType(typeof(IEnumerable<StyleDetailsAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetStyleDetailsByBuyerOrderTypeAsync(
           [FromRoute] int buyerCode,
           [FromRoute] string order,
           [FromRoute] int typeCode)
        {
            var styles = await _styleDetailsService.GetStyleDetailsByBuyerOrderTypeAsync(buyerCode, order, typeCode);
            if (styles == null)
            {
                return UnprocessableEntity("Styles not available for code :" + buyerCode +order);
            }
            var styleDetailsAPIModels = _mapper.Map<IEnumerable<StyleDetailsAPIModel>>(styles);
            return Ok(styles);
        }

        [HttpPut("/paging")]
        [Authorize(Policy = "style-details")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> UpdateStyleDetailsAsync([FromQuery] int buyerCode, string order, int typeCode, string style, [FromBody] UpdateStyleAPIModel
          updateStyleAPIModel)
        {
            var resultStyleAPIModel = _mapper.Map<StyleAPIModel>(await _styleDetailsService.GetStyleDetailsByBuyerOrderTypeStyleAsync(buyerCode,order, typeCode, style));

            if (resultStyleAPIModel == null)
            {
                return UnprocessableEntity("Style is not available for Buyer/Order :" + buyerCode + "/" + order);
            }
            updateStyleAPIModel.Id = resultStyleAPIModel.Id;
            var updateStyleSeviceModel = _mapper.Map<UpdateStyleDetailsServiceModel>(updateStyleAPIModel);

            try
            {
                var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                await _styleDetailsService.UpdateStyleDetailsAsync(updateStyleSeviceModel, currentUserEmail);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete()]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteCountryAsync(int buyer, string order, int type, string _style)
        {
            var style = await _styleDetailsService.GetStyleDetailsByBuyerOrderTypeStyleAsync(buyer, order,type, _style);
            if (style == null)
            {
                return UnprocessableEntity("Style is not available for code :" + _style);
            }
            await _styleDetailsService.DeleteStyleDetailsAsync(buyer, order, type, _style);
            return NoContent();
        }
    }
}
