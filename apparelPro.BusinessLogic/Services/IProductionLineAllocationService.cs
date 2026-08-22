using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IProductionLineAllocationService
    {
        Task<List<ProductionLineAllocationServiceModel>> GetByShipmentAsync(
            int buyerCode, string order, int typeCode, string styleCode, string shipmentOrder);

        // Used by Actual Production Entry to warn the user, before saving,
        // whether today's date is going to slip this style's schedule on
        // this line - mirrors exactly which allocation DailyProductionEntryService
        // itself treats as "the current slot" (last by start date).
        Task<List<ProductionLineAllocationServiceModel>> GetByLineAsync(
            int buyerCode, string order, int typeCode, string styleCode, string lineCode);

        Task<ProductionLineAllocationServiceModel> ManualAllocateAsync(
            ManualAllocateProductionLineServiceModel model);

        Task<ProductionLineAllocationResultServiceModel> AutomaticAllocateAsync(
            AutomaticAllocateProductionLineServiceModel model);

        Task DeleteAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            string shipmentOrder, string lineCode);
    }
}
