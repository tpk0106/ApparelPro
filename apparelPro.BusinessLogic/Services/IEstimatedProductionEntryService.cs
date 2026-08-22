using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionEntryService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IEstimatedProductionEntryService
    {
        Task<List<EstimatedProductionEntryServiceModel>> GetByLineAsync(
            int buyerCode, string order, int typeCode, string styleCode, string lineCode);

        Task<List<EstimatedProductionEntryServiceModel>> BulkSaveAsync(
            int buyerCode, string order, int typeCode, string styleCode, string lineCode,
            List<CreateEstimatedProductionEntryServiceModel> records);
    }
}
