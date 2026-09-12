using apparelPro.BusinessLogic.Services.Models.ImportExport.IPaymentTermService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPaymentTermService
    {
        Task<PaginationResult<PaymentTermServiceModel>> GetPaymentTermsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<PaymentTermServiceModel> GetPaymentTermByCodeAsync(string code);
        Task<PaymentTermServiceModel> AddPaymentTermAsync(CreatePaymentTermServiceModel createPaymentTermServiceModel);
        Task UpdatePaymentTermAsync(UpdatePaymentTermServiceModel updatePaymentTermServiceModel);
        Task DeletePaymentTermAsync(string code);
    }
}
