using apparelPro.BusinessLogic.Services.Models.OrderManagement.IGarmentAdditionalCostService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IGarmentAdditionalCostService
    {
        Task<List<GarmentAdditionalCostServiceModel>> GetGarmentAdditionalCostsAsync(
            int buyerCode, string order, int typeCode, string styleCode);

        Task<GarmentAdditionalCostServiceModel> SaveGarmentAdditionalCostAsync(
            SaveGarmentAdditionalCostServiceModel request);

        Task<bool> DeleteGarmentAdditionalCostAsync(
            int buyerCode, string order, int typeCode, string styleCode, string additionalCostCode, string itemCode);

        Task<GarmentAdditionalCostReportServiceModel> GetGarmentAdditionalCostReportAsync(
            int buyerCode, string order, int typeCode, string styleCode);
    }
}
