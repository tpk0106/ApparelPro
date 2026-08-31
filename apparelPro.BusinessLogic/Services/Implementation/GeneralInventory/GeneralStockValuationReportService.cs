using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_SVAL.PRG - "STOCK VALUATION REPORT". Unlike the
    // Status/Movement reports, this is a plain current-snapshot listing straight off
    // GeneralStockMasters for a Store and an Item Code range - no month/date filter, no
    // transaction replay. Legacy has no "no data in range" error (an empty range just
    // prints a zero Total Value), so this deliberately doesn't throw for zero rows,
    // unlike GetHeaderAsync on the Status/Movement reports.
    public class GeneralStockValuationReportService : IGeneralStockValuationReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public GeneralStockValuationReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<(GeneralStockValuationReportHeaderServiceModel Header, List<GeneralStockValuationReportLineServiceModel> Lines)> BuildAsync(
            string storeCode, string fromItemCode, string toItemCode)
        {
            storeCode = storeCode.Trim().ToUpper();
            fromItemCode = fromItemCode.Trim();
            toItemCode = toItemCode.Trim();

            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == storeCode);
            if (store == null)
                throw new KeyNotFoundException($"Invalid Stores Code '{storeCode}'.");

            bool fromItemExists = await _apparelProDbContext.GeneralStockReferences.AsNoTracking().AnyAsync(r => r.ItemCode == fromItemCode);
            if (!fromItemExists)
                throw new KeyNotFoundException($"Invalid From Item Code '{fromItemCode}'.");

            bool toItemExists = await _apparelProDbContext.GeneralStockReferences.AsNoTracking().AnyAsync(r => r.ItemCode == toItemCode);
            if (!toItemExists)
                throw new KeyNotFoundException($"Invalid To Item Code '{toItemCode}'.");

            var masters = await _apparelProDbContext.GeneralStockMasters
                .AsNoTracking()
                .Where(m => m.StoreCode == storeCode && string.Compare(m.ItemCode, fromItemCode) >= 0 && string.Compare(m.ItemCode, toItemCode) <= 0)
                .OrderBy(m => m.ItemCode)
                .ToListAsync();

            var itemCodes = masters.Select(m => m.ItemCode).ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            var lines = masters.Select(m => new GeneralStockValuationReportLineServiceModel
            {
                ItemCode = m.ItemCode,
                Description = descriptions.GetValueOrDefault(m.ItemCode, ""),
                Unit = m.Unit,
                QtyInHand = m.QtyInHand,
                Value = m.Value,
                DamagedQuantity = m.DamagedQuantity,
                ReorderLevel = m.ReorderLevel,
                ReorderQuantity = m.ReorderQuantity,
                MinStock = m.MinStock,
                MaxStock = m.MaxStock,
                UnitPrice = m.QtyInHand != 0 ? m.Value / m.QtyInHand : 0,
                Currency = m.Currency,
            }).ToList();

            var header = new GeneralStockValuationReportHeaderServiceModel
            {
                StoreCode = storeCode,
                StoreDescription = store.Description,
                FromItemCode = fromItemCode,
                ToItemCode = toItemCode,
                TotalValue = lines.Sum(l => l.Value),
                TotalLineItems = lines.Count,
            };

            return (header, lines);
        }

        public async Task<GeneralStockValuationReportHeaderServiceModel> GetHeaderAsync(string storeCode, string fromItemCode, string toItemCode)
        {
            var (header, _) = await BuildAsync(storeCode, fromItemCode, toItemCode);
            return header;
        }

        public async Task<List<GeneralStockValuationReportLineServiceModel>> GetLinesAsync(string storeCode, string fromItemCode, string toItemCode)
        {
            var (_, lines) = await BuildAsync(storeCode, fromItemCode, toItemCode);
            return lines;
        }
    }
}
