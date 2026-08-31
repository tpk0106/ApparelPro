using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IRawMaterialControlSheetService
    {
        Task<RawMaterialControlSheetHeaderServiceModel> GetHeaderAsync(int buyerCode, string order);
        Task<List<RawMaterialControlSheetLineServiceModel>> GetLinesAsync(int buyerCode, string order);
    }
}
