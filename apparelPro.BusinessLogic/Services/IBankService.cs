using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IBankService
    {
        Task<PaginationResult<BankServiceModel>> GetBanksAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        //   Task<PaginationResult<BankServiceModel>> GetCountriesAsync(int pageNumber, int pageSize, string? filter, string? sortColumn, bool? descending);

        Task<IEnumerable<BankServiceModel>> GetBanksByPageNumberAsync(int pageNumber, int pageSize);

        Task<IEnumerable<BankServiceModel>> FilterBanksByCodeAsync(string filter, int pageNumber, int pageSize);
        Task<BankServiceModel> GetBankByCodeAsync(string code);
        Task<bool> DoesBankExistAsync(string code);
        Task<BankServiceModel> AddBankAsync(CreateBankServiceModel createBankServiceModel);
        Task UpdateBankAsync(UpdateBankServiceModel updateBankServiceModel);
        Task DeleteBankAsync(string code);
        //Task<bool> DoesUnitExistAsync(string code);
    }
}
