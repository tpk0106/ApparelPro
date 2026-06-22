using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using ApparelPro.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apparelPro.BusinessLogic.Services
{
    public interface ISupplierService
    {
        Task<PaginationResult<SupplierServiceModel>> GetSuppliersAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);

        Task<List<SupplierLookupServiceModel>> GetSuppliersLookupAsync();
        Task<SupplierServiceModel> GetSupplierBySupplierCodeAsync(int supplierCode);        
        Task<SupplierServiceModel> AddSupplierAsync(CreateSupplierServiceModel createSupplierServiceModel);
        Task UpdateSupplierAsync(UpdateSupplierServiceModel  updateSupplierServiceModel);
        Task DeleteSupplierAsync(int buyerCode);
    }
}
