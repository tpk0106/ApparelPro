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
        private readonly IAddressService _addressService;
        public AddressController(IMapper mapper, IAddressService addressService)
        {
            _mapper = mapper;
            _addressService = addressService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "address-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<AddressAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAllBuyerAddressesAsync(
             [FromQuery] int pageSize,
             [FromQuery] int pageNumber,
             [FromQuery] string? sortColumn = null,
             [FromQuery] string? sortOrder = null,
             [FromQuery] string? filterColumn = null,
             [FromQuery] string? filterQuery = null)
        {
            var addressServiceModels = await _addressService.GetAllBuyerAddressesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var addresses = _mapper.Map<PaginationAPIModel<AddressAPIModel>>(addressServiceModels);
            return Ok(addresses);
        }

        [HttpGet("list/AddressId/{addressId}", Name = "GetAddressesByAddressIdAsync")]
        [Authorize(Policy = "address-view")]
        [ProducesResponseType(typeof(IEnumerable<AddressAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAddressesByAddressIdAsync(Guid addressId)
        {
            var addresseServiceModels = await _addressService.GetAddressesByAddresIdAsync(addressId);

            var addresses = _mapper.Map<IEnumerable<AddressAPIModel>>(addresseServiceModels);
            return Ok(addresses);
        }

        [HttpGet("list/buyerCode/", Name = "GetAddressesByBuyerCodeAsync")]
        [Authorize(Policy = "address-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<AddressAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAddressesByBuyerCodeAsync(
            [FromQuery] int buyerCode,
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
       )
        {
            var addresseServiceModels = await _addressService.GetAddressesByBuyerCodeAsync(buyerCode, pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var addresses = _mapper.Map<PaginationAPIModel<AddressAPIModel>>(addresseServiceModels);
            return Ok(addresses);
        }

        [HttpGet("list/bankCode/", Name = "GetAddressesByBankCodeAsync")]
        [Authorize(Policy = "address-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<AddressAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAddressesByBankCodeAsync(
            [FromQuery] string bankCode,
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
       )
        {
            var addresseServiceModels = await _addressService.GetAddressesByBankCodeAsync(bankCode, pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var addresses = _mapper.Map<PaginationAPIModel<AddressAPIModel>>(addresseServiceModels);
            return Ok(addresses);
        }

        [HttpGet("list/byAddressId/", Name = "GetAddressesForBuyerByAddressIdAsync")]
        [Authorize(Policy = "address-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<AddressAPIModel>), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAddressesForBuyerByAddressIdAsync(
             [FromQuery] Guid addressId,
             [FromQuery] int pageSize,
             [FromQuery] int pageNumber,
             [FromQuery] string? sortColumn = null,
             [FromQuery] string? sortOrder = null,
             [FromQuery] string? filterColumn = null,
             [FromQuery] string? filterQuery = null
        )
        {
            var addresseServiceModels = await _addressService.GetAddressesByAddresIdAsync(addressId,pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var addresses = _mapper.Map<PaginationAPIModel<AddressAPIModel>>(addresseServiceModels);
            return Ok(addresses);
        }

        [HttpGet("list/byIdAndAddressId/", Name = "GetAddressByIdAndAddresIdAsync")]
        [Authorize(Policy = "address-view")]
        [ProducesResponseType(typeof(AddressAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetAddressByIdAndAddresIdAsync([FromQuery] Guid addressId, [FromQuery] int id)
        {
            var addresseServiceModel = await _addressService.GetAddressByIdAndAddresIdAsync(id, addressId);
            var address = _mapper.Map<AddressAPIModel>(addresseServiceModel);
            return Ok(address);
        }

        [HttpPost]
        [Authorize(Policy = "address-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddAddressAsync([FromBody] CreateAddressAPIModel createAddressAPIModel)
        {
            var createAddressServiceModel = _mapper.Map<CreateAddressServiceModel>(createAddressAPIModel);
            var addedAddress = await _addressService.AddAddressAsync(createAddressServiceModel);
            return CreatedAtRoute(nameof(GetAddressesByAddressIdAsync), new {  addedAddress.Id, addedAddress.AddressId }, null);
        }

        [HttpPut()]
        [Authorize(Policy = "address-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateAddressAsync([FromQuery] int id, Guid addressId, [FromBody] UpdateAddressAPIModel
         updateAddressAPIModel)
        {
            var resultAddressAPIModel = _mapper.Map<AddressAPIModel>(await _addressService.GetAddressByIdAndAddresIdAsync(id, addressId));

            if (resultAddressAPIModel == null)
            {
                return UnprocessableEntity("Address is not available for code :" + addressId);
            }

            var updateAddressSeviceModel = _mapper.Map<UpdateAddressServiceModel>(updateAddressAPIModel);
            await _addressService.UpdateAddressAsync(updateAddressSeviceModel);
            return NoContent();
        }

        [HttpPut("{buyerCode}/{addressId}")]
        [Authorize(Policy = "address-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateAddressByBuyerCodeAsync([FromRoute] int buyerCode, Guid addressId,
            [FromBody] UpdateAddressAPIModel updateAddressAPIModel)
        {
            var resultAddressAPIModel = _mapper.Map<AddressAPIModel>(
                await _addressService.GetAddressByBuyerCodeAndAddresIdAsync(buyerCode, addressId)
            );

            if (resultAddressAPIModel == null)
            {
                return UnprocessableEntity("Address is not available for buyercode :" + buyerCode);
            }

            resultAddressAPIModel.AddressId = addressId;
            resultAddressAPIModel.BuyerCode = buyerCode;
            resultAddressAPIModel.StreetAddress = updateAddressAPIModel.StreetAddress;
            resultAddressAPIModel.AddressType = updateAddressAPIModel.AddressType;
            resultAddressAPIModel.State = updateAddressAPIModel.State;
            resultAddressAPIModel.City = updateAddressAPIModel.City;
            resultAddressAPIModel.CountryCode = updateAddressAPIModel.CountryCode;
            resultAddressAPIModel.PostCode = updateAddressAPIModel.PostCode;
            resultAddressAPIModel.Default = updateAddressAPIModel.Default;

            var updateAddressSeviceModel = _mapper.Map<UpdateAddressServiceModel>(resultAddressAPIModel);
            await _addressService.UpdateDefaultAddressAsync(updateAddressSeviceModel);

            return NoContent();
        }

        [HttpPut("bank/{bankCode}/{addressId}")]
        [Authorize(Policy = "address-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateAddressByBankCodeAsync([FromRoute] string bankCode, Guid addressId,
            [FromBody] UpdateAddressAPIModel updateAddressAPIModel)
        {
            var resultAddressAPIModel = _mapper.Map<AddressAPIModel>(
                await _addressService.GetAddressByBankCodeAndAddresIdAsync(bankCode, addressId)
            );

            if (resultAddressAPIModel == null)
            {
                return UnprocessableEntity("Address is not available for bank code :" + bankCode);
            }

            resultAddressAPIModel.AddressId = addressId;
            resultAddressAPIModel.BankCode = bankCode;
            resultAddressAPIModel.StreetAddress = updateAddressAPIModel.StreetAddress;
            resultAddressAPIModel.AddressType = updateAddressAPIModel.AddressType;
            resultAddressAPIModel.State = updateAddressAPIModel.State;
            resultAddressAPIModel.City = updateAddressAPIModel.City;
            resultAddressAPIModel.CountryCode = updateAddressAPIModel.CountryCode;
            resultAddressAPIModel.PostCode = updateAddressAPIModel.PostCode;
            resultAddressAPIModel.Default = updateAddressAPIModel.Default;

            var updateAddressSeviceModel = _mapper.Map<UpdateAddressServiceModel>(resultAddressAPIModel);
            await _addressService.UpdateDefaultAddressByBankAsync(updateAddressSeviceModel);

            return NoContent();
        }

        [HttpDelete("{id}/{addressId}")]
        [Authorize(Policy = "address-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteBuyerAddressAsync(int id, string addressId)
        {
            var newAddressId = new Guid(addressId);
            var buyerAddress = await _addressService.GetAddressByIdAndAddresIdAsync(id,newAddressId);
            if (buyerAddress == null)
            {
                return UnprocessableEntity("Buyer Address is not available for AddressId :" + addressId);
            }
            await _addressService.DeleteAddressAsync(id, newAddressId);
            return NoContent();
        }
    }
}
