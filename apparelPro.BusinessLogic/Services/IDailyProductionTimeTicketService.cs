using apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionTimeTicketService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IDailyProductionTimeTicketService
    {
        Task<DailyProductionTimeTicketServiceModel> GetByTicketAsync(
            DateOnly date, string lineCode, int buyerCode, string order, int typeCode, string styleCode);

        Task<DailyProductionTimeTicketServiceModel> BulkSaveAsync(
            DateOnly date, string lineCode, int buyerCode, string order, int typeCode, string styleCode,
            List<CreateDailyProductionTimeTicketEntryServiceModel> records);
    }
}
