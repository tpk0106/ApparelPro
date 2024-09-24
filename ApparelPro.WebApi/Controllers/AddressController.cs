using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.Shared;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
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

        [HttpGet("list/byAddressId/", Name = "GetAddressesByAddressIdAsync")]
        [Authorize("Merchandising")]
        [ProducesResponseType(typeof(PaginationAPIModel<AddressAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAddressesByAddressIdAsync(
             [FromQuery] Guid addressId,
             [FromQuery] int pageSize,
             [FromQuery] int pageNumber,
             [FromQuery] string? sortColumn = null,
             [FromQuery] string? sortOrder = null,
             [FromQuery] string? filterColumn = null,
             [FromQuery] string? filterQuery = null
        )           
        {
            var addresseServiceModels = await _addresService.GetAddressesByAddresIdAsync(addressId,pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var addresses = _mapper.Map<PaginationAPIModel<AddressAPIModel>>(addresseServiceModels);
            return Ok(addresses);
        }

        [HttpGet("list/byIdAndAddressId/", Name = "GetAddressByIdAndAddresIdAsync")]
        [Authorize("Merchandising")]
        [ProducesResponseType(typeof(AddressAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAddressByIdAndAddresIdAsync([FromQuery] Guid addressId, [FromQuery] int id)
        {
            var addresseServiceModel = await _addresService.GetAddressByIdAndAddresIdAsync(id, addressId);
            var address = _mapper.Map<AddressAPIModel>(addresseServiceModel);
            return Ok(address);
        }

        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddAddressAsync([FromBody] CreateAddressAPIModel createAddressAPIModel)
        {
            var createAddressServiceModel = _mapper.Map<CreateAddressServiceModel>(createAddressAPIModel);
            var addedAddress = await _addresService.AddAddressAsync(createAddressServiceModel);
            return CreatedAtRoute(nameof(GetAddressesByAddressIdAsync), new {  addedAddress.Id, addedAddress.AddressId }, null);
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        //  [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateAddressAsync([FromQuery] int id, Guid addressId, [FromBody] UpdateAddressAPIModel
            updateAddressAPIModel)
        {
            var resultAddressAPIModel = _mapper.Map<AddressAPIModel>(await _addresService.GetAddressByIdAndAddresIdAsync(id, addressId));

            if (resultAddressAPIModel == null)
            {
                return UnprocessableEntity("Address is not available for code :" + addressId);
            }           
          
            var updateAddressSeviceModel = _mapper.Map<UpdateAddressServiceModel>(updateAddressAPIModel);
            await _addresService.UpdateAddressAsync(updateAddressSeviceModel);
            return NoContent();
        }
    }
}
