using apparelPro.BusinessLogic.Services.Models.OrderManagement.ISubContractService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ISubContractService
    {
        Task<List<SubContractServiceModel>> GetSubContractsAsync(
            int buyerCode, string order, int typeCode, string styleCode);

        Task<SaveSubContractResultServiceModel> SaveSubContractAsync(
            SaveSubContractServiceModel request);

        Task<bool> DeleteSubContractAsync(
            int buyerCode, string order, int typeCode, string styleCode, string subContractorCode);
    }
}
