using apparelPro.BusinessLogic.Services.Implementation.Shared;

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
        Task<AddressServiceModel> GetAddressByIdAndAddresIdAsync(int id, Guid addressId);

        Task<IEnumerable<AddressServiceModel>> FilterAddressesByCodeAsync(string filter, int pageNumber, int pageSize);
       // Task<AddressServiceModel> GetAddressByCodeAsync(string code);
        Task<bool> DoesAddressExistAsync(string code);
        Task<AddressServiceModel> AddAddressAsync(CreateAddressServiceModel createAddressServiceModel);
        Task UpdateAddressAsync(UpdateAddressServiceModel updateAddressServiceModel);
        Task DeleteAddressAsync(int id, Guid addressId);
        //Task<bool> DoesUnitExistAsync(string code);
    }
}
