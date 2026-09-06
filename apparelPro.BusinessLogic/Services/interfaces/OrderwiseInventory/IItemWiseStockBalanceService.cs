using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IItemWiseStockBalanceService
    {
        Task<ItemWiseStockBalanceHeaderServiceModel> GetHeaderAsync(string fromRange, string toRange);
        Task<List<ItemWiseStockBalanceLineServiceModel>> GetLinesAsync(string fromRange, string toRange);
        Task<List<ItemCodeSearchResultServiceModel>> SearchItemCodesAsync(string query);
    }
}
