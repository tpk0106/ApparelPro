using apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionEntryService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IDailyProductionEntryService
    {
        Task<List<DailyProductionEntryServiceModel>> GetByDateAsync(
            DateOnly date, int buyerCode, string order, int typeCode, string styleCode, string lineCode);

        Task<List<DailyProductionEntryServiceModel>> BulkSaveAsync(
            DateOnly date, int buyerCode, string order, int typeCode, string styleCode, string lineCode,
            List<CreateDailyProductionEntryServiceModel> records);
    }
}
