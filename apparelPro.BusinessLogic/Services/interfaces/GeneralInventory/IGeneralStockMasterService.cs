using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralStockMasterService
    {
        Task<bool> CommitGeneralStockMasterAsync(GeneralStockMasterEntryServiceModel model);

        // Edits an EXISTING row's metadata (Description/Unit/Currency/Reorder levels)
        // by its already-known composite ItemCode - deliberately does not re-validate
        // against the StockItems catalog the way CommitGeneralStockMasterAsync does,
        // since rows bulk-imported from legacy gi_stmst.dbf data may not have a
        // matching StockItems/OrderItemFeatures entry at all. The row already existing
        // in GeneralStockMasters is itself sufficient proof it's a real item.
        Task<bool> UpdateGeneralStockMasterAsync(GeneralStockMasterUpdateServiceModel model);

        Task<List<GeneralStockMasterRowServiceModel>> GetGeneralStockMastersByStoreAsync(string storeCode);

        Task DeleteGeneralStockMasterAsync(string storeCode, string itemCode);
    }
}
