using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_TPDT1.PRG - "GENERAL STOCK MASTER CREATION". Every
    // other General Inventory note type requires a GeneralStockMaster row to already
    // exist before it can move any stock (STRN/GIN/GRN/GTN/RTN/DGN/SRTN/PO all validate
    // against it, never auto-create) - this is the one screen that actually creates that
    // row, same as legacy. StockCode/ItemCode/Feature slots reuse the exact catalog
    // Order Management's Material Consumption screen already uses (StockItems /
    // OrderItemFeatures / ItemFeatures via api/material-consumption/catalog and
    // api/material-consumption/feature-headers) - General Inventory items are drawn
    // from the same shared item catalog, not a separate one.
    public class GeneralStockMasterService : IGeneralStockMasterService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public GeneralStockMasterService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        // Same fixed-width composite convention as MaterialConsumptionService's own
        // ComposeCostProfileItemCode - StockCode(2) + ItemCode(4) + Feature1-4(4 each) = 22 chars.
        private static string ComposeItemCode(string stockCode, string itemCode, string? f1, string? f2, string? f3, string? f4)
        {
            static string Segment(string? value, int width) => (value ?? string.Empty).Trim().PadRight(width).Substring(0, width);
            return Segment(stockCode, 2) + Segment(itemCode, 4) + Segment(f1, 4) + Segment(f2, 4) + Segment(f3, 4) + Segment(f4, 4);
        }

        public async Task<bool> CommitGeneralStockMasterAsync(GeneralStockMasterEntryServiceModel model)
        {
            model.StoreCode = model.StoreCode.Trim().ToUpper();
            model.StockCode = model.StockCode.Trim();
            model.ItemCode = model.ItemCode.Trim();
            model.Unit = model.Unit.Trim().ToUpper();
            model.CurrencyCode = model.CurrencyCode.Trim().ToUpper();

            var storeExists = await _apparelProDbContext.GeneralStores.AsNoTracking().AnyAsync(s => s.Code == model.StoreCode);
            if (!storeExists)
                throw new InvalidOperationException($"Invalid Stores Code '{model.StoreCode}'.");

            var stockItemExists = await _apparelProDbContext.StockItems.AsNoTracking()
                .AnyAsync(i => i.StockCode == model.StockCode && i.ItemCode == model.ItemCode);
            if (!stockItemExists)
                throw new InvalidOperationException($"Invalid Stock/Item Code '{model.StockCode}/{model.ItemCode}'.");

            var unitExists = await _apparelProDbContext.Units.AsNoTracking().AnyAsync(u => u.Code == model.Unit);
            if (!unitExists)
                throw new InvalidOperationException($"Invalid Unit Code '{model.Unit}'.");

            var currencyExists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == model.CurrencyCode);
            if (!currencyExists)
                throw new InvalidOperationException($"Invalid Currency Code '{model.CurrencyCode}'.");

            string compositeItemCode = ComposeItemCode(model.StockCode, model.ItemCode, model.Feature1, model.Feature2, model.Feature3, model.Feature4);

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                var stockReference = await _apparelProDbContext.GeneralStockReferences.FirstOrDefaultAsync(r => r.ItemCode == compositeItemCode);
                if (stockReference == null)
                {
                    await _apparelProDbContext.GeneralStockReferences.AddAsync(new GeneralStockReference
                    {
                        ItemCode = compositeItemCode,
                        Description = model.Description.Trim(),
                    });
                }
                else
                {
                    stockReference.Description = model.Description.Trim();
                    _apparelProDbContext.GeneralStockReferences.Update(stockReference);
                }

                var stockMaster = await _apparelProDbContext.GeneralStockMasters
                    .FirstOrDefaultAsync(m => m.StoreCode == model.StoreCode && m.ItemCode == compositeItemCode);
                if (stockMaster == null)
                {
                    await _apparelProDbContext.GeneralStockMasters.AddAsync(new GeneralStockMaster
                    {
                        StoreCode = model.StoreCode,
                        ItemCode = compositeItemCode,
                        Unit = model.Unit,
                        Currency = model.CurrencyCode,
                        ReorderLevel = model.ReorderLevel,
                        ReorderQuantity = model.ReorderQuantity,
                        MinStock = model.MinStock,
                        MaxStock = model.MaxStock,
                        QtyInHand = 0,
                        ShadowBalance = 0,
                        Value = 0,
                        DamagedQuantity = 0,
                    });
                }
                else
                {
                    // Master-data fields only - QtyInHand/Value/ShadowBalance/DamagedQuantity are
                    // ledger-derived and never touched by this screen, matching legacy (GI_TPDT1.PRG's
                    // commit "repl" list never includes qty_in_hd/value/shdw_bal/dam_qty).
                    stockMaster.Unit = model.Unit;
                    stockMaster.Currency = model.CurrencyCode;
                    stockMaster.ReorderLevel = model.ReorderLevel;
                    stockMaster.ReorderQuantity = model.ReorderQuantity;
                    stockMaster.MinStock = model.MinStock;
                    stockMaster.MaxStock = model.MaxStock;
                    _apparelProDbContext.GeneralStockMasters.Update(stockMaster);
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return true;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateGeneralStockMasterAsync(GeneralStockMasterUpdateServiceModel model)
        {
            model.StoreCode = model.StoreCode.Trim().ToUpper();
            model.ItemCode = model.ItemCode.Trim();
            model.Unit = model.Unit.Trim().ToUpper();
            model.CurrencyCode = model.CurrencyCode.Trim().ToUpper();

            var unitExists = await _apparelProDbContext.Units.AsNoTracking().AnyAsync(u => u.Code == model.Unit);
            if (!unitExists)
                throw new InvalidOperationException($"Invalid Unit Code '{model.Unit}'.");

            var currencyExists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == model.CurrencyCode);
            if (!currencyExists)
                throw new InvalidOperationException($"Invalid Currency Code '{model.CurrencyCode}'.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                var stockMaster = await _apparelProDbContext.GeneralStockMasters
                    .FirstOrDefaultAsync(m => m.StoreCode == model.StoreCode && m.ItemCode == model.ItemCode);
                if (stockMaster == null)
                    throw new KeyNotFoundException($"Item '{model.ItemCode}' not found in Stores '{model.StoreCode}'.");

                stockMaster.Unit = model.Unit;
                stockMaster.Currency = model.CurrencyCode;
                stockMaster.ReorderLevel = model.ReorderLevel;
                stockMaster.ReorderQuantity = model.ReorderQuantity;
                stockMaster.MinStock = model.MinStock;
                stockMaster.MaxStock = model.MaxStock;
                _apparelProDbContext.GeneralStockMasters.Update(stockMaster);

                var stockReference = await _apparelProDbContext.GeneralStockReferences
                    .FirstOrDefaultAsync(r => r.ItemCode == model.ItemCode);
                if (stockReference == null)
                {
                    await _apparelProDbContext.GeneralStockReferences.AddAsync(new GeneralStockReference
                    {
                        ItemCode = model.ItemCode,
                        Description = model.Description.Trim(),
                    });
                }
                else
                {
                    stockReference.Description = model.Description.Trim();
                    _apparelProDbContext.GeneralStockReferences.Update(stockReference);
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return true;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<GeneralStockMasterRowServiceModel>> GetGeneralStockMastersByStoreAsync(string storeCode)
        {
            storeCode = storeCode.Trim().ToUpper();

            var masters = await _apparelProDbContext.GeneralStockMasters
                .AsNoTracking()
                .Where(m => m.StoreCode == storeCode)
                .OrderBy(m => m.ItemCode)
                .ToListAsync();

            var itemCodes = masters.Select(m => m.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return masters.Select(m => new GeneralStockMasterRowServiceModel
            {
                StoreCode = m.StoreCode,
                ItemCode = m.ItemCode,
                Description = descriptions.GetValueOrDefault(m.ItemCode, ""),
                Unit = m.Unit,
                Currency = m.Currency,
                ReorderLevel = m.ReorderLevel,
                ReorderQuantity = m.ReorderQuantity,
                MinStock = m.MinStock,
                MaxStock = m.MaxStock,
                QtyInHand = m.QtyInHand,
            }).ToList();
        }

        public async Task DeleteGeneralStockMasterAsync(string storeCode, string itemCode)
        {
            storeCode = storeCode.Trim().ToUpper();
            itemCode = itemCode.Trim();

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                var stockMaster = await _apparelProDbContext.GeneralStockMasters
                    .FirstOrDefaultAsync(m => m.StoreCode == storeCode && m.ItemCode == itemCode);
                if (stockMaster == null)
                    throw new KeyNotFoundException($"Item '{itemCode}' not found in Stores '{storeCode}'.");

                if (stockMaster.QtyInHand > 0)
                    throw new InvalidOperationException("Stock Balances remaining! Cannot Delete.");

                _apparelProDbContext.GeneralStockMasters.Remove(stockMaster);

                var stockReference = await _apparelProDbContext.GeneralStockReferences.FirstOrDefaultAsync(r => r.ItemCode == itemCode);
                if (stockReference != null)
                    _apparelProDbContext.GeneralStockReferences.Remove(stockReference);

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
    }
}
