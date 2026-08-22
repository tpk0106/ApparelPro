using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionLineAllocationService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IEstimatedProductionLineAllocationService
    {
        Task<EstimatedProductionLineAllocationServiceModel?> GetAsync(int buyerCode, string styleCode);

        Task<EstimatedProductionLineAllocationServiceModel> ManualAllocateAsync(
            ManualAllocateEstimatedProductionLineServiceModel model);

        Task<EstimatedProductionLineAllocationResultServiceModel> AutomaticAllocateAsync(
            AutomaticAllocateEstimatedProductionLineServiceModel model);

        Task DeleteAsync(int buyerCode, string styleCode);
    }
}
