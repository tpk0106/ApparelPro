using apparelPro.BusinessLogic.Services.Implementation.Shared;
using apparelPro.BusinessLogic.Services.Models.Shared.IAddressService;
using ApparelPro.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apparelPro.BusinessLogic.Services
{
    public interface IAddressService
    {
        Task<PaginationResult<AddressServiceModel>> GetAddressesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        //   Task<PaginationResult<AddressServiceModel>> GetAddressesAsync(int pageNumber, int pageSize, string? filter, string? sortColumn, bool? descending);

        Task<PaginationResult<AddressServiceModel>> GetAddressesByAddresIdAsync(Guid addressId, int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<IEnumerable<AddressServiceModel>> GetAddressesByAddresIdAsync(Guid addressId);
        Task<AddressServiceModel> GetAddressByIdAndAddresIdAsync(int id, Guid addressId);

        Task<AddressServiceModel> GetAddressByBuyerCodeAndAddresIdAsync(int buyerCode, Guid addressId);
        Task<PaginationResult<AddressServiceModel>> GetAddressesByBuyerCodeAsync(int buyerCode, int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        //  Task<AddressServiceModel> GetAddressByIdAndAddresIdAsync(int id, string addressId);

        Task<IEnumerable<AddressServiceModel>> FilterAddressesByCodeAsync(string filter, int pageNumber, int pageSize);
       // Task<AddressServiceModel> GetAddressByCodeAsync(string code);
        Task<bool> DoesAddressExistAsync(string code);
        Task<AddressServiceModel> AddAddressAsync(CreateAddressServiceModel createAddressServiceModel);
        Task UpdateAddressAsync(UpdateAddressServiceModel updateAddressServiceModel);

        Task UpdateDefaultAddressAsync(UpdateAddressServiceModel updateAddressServiceModel);

        //Task UpdateAddressAsync(UpdateDefaultAddressServiceModel updateAddressServiceModel);

        Task DeleteAddressAsync(int id, Guid addressId);
        //Task DeleteAddressAsync(int id, string addressId);
        //Task<bool> DoesUnitExistAsync(string code);
    }
}
