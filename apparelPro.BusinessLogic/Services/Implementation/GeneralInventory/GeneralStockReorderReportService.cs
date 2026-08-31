using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_ROL.PRG - "STOCK RE-ORDER REPORT". Current-snapshot
    // listing straight off GeneralStockMasters for a Store, filtered to items where
    // QtyInHand <= ReorderLevel - no month/date filter, no transaction replay (same
    // family as GeneralStockValuationReportService).
    public class GeneralStockReorderReportService : IGeneralStockReorderReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public GeneralStockReorderReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<(GeneralStockReorderReportHeaderServiceModel Header, List<GeneralStockReorderReportLineServiceModel> Lines)> BuildAsync(string storeCode)
        {
            storeCode = storeCode.Trim().ToUpper();

            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == storeCode);
            if (store == null)
                throw new KeyNotFoundException($"Invalid Stores Code '{storeCode}'.");

            bool storeHasAnyItems = await _apparelProDbContext.GeneralStockMasters.AsNoTracking().AnyAsync(m => m.StoreCode == storeCode);
            if (!storeHasAnyItems)
                throw new InvalidOperationException("No Items for given Stores.");

            var masters = await _apparelProDbContext.GeneralStockMasters
                .AsNoTracking()
                .Where(m => m.StoreCode == storeCode && m.QtyInHand <= m.ReorderLevel)
                .OrderBy(m => m.ItemCode)
                .ToListAsync();

            var itemCodes = masters.Select(m => m.ItemCode).ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            var lines = masters.Select(m => new GeneralStockReorderReportLineServiceModel
            {
                ItemCode = m.ItemCode,
                Description = descriptions.GetValueOrDefault(m.ItemCode, ""),
                Unit = m.Unit,
                AveragePrice = m.QtyInHand != 0 ? m.Value / m.QtyInHand : 0,
                QtyInHand = m.QtyInHand,
                ReorderLevel = m.ReorderLevel,
                ReorderQuantity = m.ReorderQuantity,
            }).ToList();

            var header = new GeneralStockReorderReportHeaderServiceModel
            {
                StoreCode = storeCode,
                StoreDescription = store.Description,
                TotalLineItems = lines.Count,
            };

            return (header, lines);
        }

        public async Task<GeneralStockReorderReportHeaderServiceModel> GetHeaderAsync(string storeCode)
        {
            var (header, _) = await BuildAsync(storeCode);
            return header;
        }

        public async Task<List<GeneralStockReorderReportLineServiceModel>> GetLinesAsync(string storeCode)
        {
            var (_, lines) = await BuildAsync(storeCode);
            return lines;
        }
    }
}
