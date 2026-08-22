using apparelPro.BusinessLogic.Services.Models.Production.IProductionProgressGraphService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IProductionProgressGraphService
    {
        Task<ProductionProgressGraphServiceModel> GetGraphAsync(int buyerCode, string order, int typeCode, string styleCode);
    }
}
