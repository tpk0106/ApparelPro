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
        [Authorize(Roles = AccessPolicies.OrderwiseInventoryStandard)]
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
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddStyleDetailsAsync([FromBody] CreateStyleDetailsAPIModel createStyleDetailsAPIModel)
        {
            var createStyleDetailsServiceModel = _mapper.Map<CreateStyleDetailsServiceModel>(createStyleDetailsAPIModel);
            var addedStyle = await _styleDetailsService.AddStyleDetailsAsync(createStyleDetailsServiceModel);
            return CreatedAtRoute(nameof(GetStyleDetailsByBuyerOrderTypeStyleAsync),
                new
                {
                    buyer = addedStyle.BuyerCode,
                    order = addedStyle.Order,
                    type = addedStyle.TypeCode,
                    style = addedStyle.StyleCode
                }, null);
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
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
            await _styleDetailsService.UpdateStyleDetailsAsync(updateStyleDetailsServiceModel); 
            
            return NoContent();
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
        [ProducesResponseType(typeof(CountryAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetStyleDetailsByBuyerOrderTypeStyleAsync(
            [FromRoute]  int buyer,
            [FromRoute] string order,
            [FromRoute] int type, 
            [FromRoute] string style)
        {
            var Bank = await _styleDetailsService.GetStyleDetailsByBuyerOrderTypeStyleAsync(buyer,order,type,style);
            if (Bank == null)
            {
                return UnprocessableEntity("Bank is not available for code :" + buyer);
            }
            var countryAPIModel = _mapper.Map<CountryAPIModel>(Bank);
            return Ok(countryAPIModel);
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
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]        
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
            await _styleDetailsService.UpdateStyleDetailsAsync(updateStyleSeviceModel);
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
