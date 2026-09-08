using apparelPro.BusinessLogic.Services.Models.ImportExport.ICompanyAddressService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICompanyAddressService
    {
        Task<List<CompanyAddressServiceModel>> GetAllAsync();
        Task<CompanyAddressServiceModel> SaveAsync(SaveCompanyAddressServiceModel serviceModel);
        Task<bool> DeleteAsync(int id);
    }
}
