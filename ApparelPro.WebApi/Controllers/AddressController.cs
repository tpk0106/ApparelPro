using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Shared;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/address")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAddressService _addresService; 
        public AddressController(IMapper mapper, IAddressService addressService)
        {
            _mapper = mapper;
            _addresService = addressService;            
        }

        [HttpGet("list")]
        //  [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied
        //[Authorize(Roles = "Inventory")]
        // [Authorize("RegisteredUser")]
        [ProducesResponseType(typeof(PaginationAPIModel<AddressAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCountriesAsync(
         [FromQuery] int pageSize,
         [FromQuery] int pageNumber,
         [FromQuery] string? sortColumn = null,
         [FromQuery] string? sortOrder = null,
         [FromQuery] string? filterColumn = null,
         [FromQuery] string? filterQuery = null)
        {
            var addressServiceModels = await _addresService.GetAddressesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var addresses = _mapper.Map<PaginationAPIModel<AddressAPIModel>>(addressServiceModels);
            return Ok(addresses);
        }


        [HttpGet("list/{addressId}", Name = "GetAddressesByAddressIdAsync")]
        [Authorize("Merchandising")]
        [ProducesResponseType(typeof(IEnumerable<AddressAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAddressesByAddressIdAsync([FromRoute] Guid addressId)
        {
            var addresses = await _addresService.GetAddressesByAddresIdAsync(addressId);
            if (addresses == null)
            {
                return UnprocessableEntity("Addresses are not available for AddressId :" + addressId);
            }
            var addressesAPIModel = _mapper.Map<IEnumerable<AddressAPIModel>>(addresses);
            return Ok(addressesAPIModel);
        }
    }
}
