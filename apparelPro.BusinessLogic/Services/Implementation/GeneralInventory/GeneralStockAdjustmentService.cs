using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_SAN1.PRG/GI_SAN2.PRG - "STOCK ADJUSTMENT NOTE
    // (General)". Unlike every other General Inventory note, this SETS QtyInHand to the
    // entered quantity (a physical stock-take correction), not an add/subtract delta -
    // mirrors legacy's "repl qty_in_hd with m_qty" verbatim. Price/currency conversion
    // uses ICurrencyConversionService the same way GeneralGoodsReceivedService already
    // does (line price may be entered in a different currency than the item master's
    // own, converted before valuing). No ShadowBalance touch, no gi_monst writes - same
    // conventions as every other General Inventory note type here.
    public class GeneralStockAdjustmentService : IGeneralStockAdjustmentService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;
        private readonly ICurrencyConversionService _currencyConversionService;

        public GeneralStockAdjustmentService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService,
            ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
            _currencyConversionService = currencyConversionService;
        }

        public async Task<bool> CommitGeneralStockAdjustmentNoteAsync(
            GeneralSanHeaderServiceModel header,
            List<GeneralSanLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Stock Adjustment Note cannot be committed without line items.");

            header.StoreCode = header.StoreCode.Trim().ToUpper();

            var storeExists = await _apparelProDbContext.GeneralStores.AsNoTracking().AnyAsync(s => s.Code == header.StoreCode);
            if (!storeExists)
                throw new InvalidOperationException($"Invalid Stores Code '{header.StoreCode}'.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string allocatedSanNumber = await _sharedService.GenerateNextDocumentNumberAsync("GSAN");

                var generalMasters = new Dictionary<string, GeneralStockMaster>();

                // Pass 1: lock and validate every line before writing anything.
                foreach (var line in lines)
                {
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();
                    line.CurrencyCode = line.CurrencyCode.Trim().ToUpper();

                    if (line.Quantity < 0)
                        throw new InvalidOperationException($"Quantity cannot be negative for Item '{line.ItemCode}'.");
                    if (line.Price <= 0)
                        throw new InvalidOperationException($"Price must be greater than zero for Item '{line.ItemCode}'.");

                    var generalMaster = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {header.StoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();
                    if (generalMaster == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' not found in Stores '{header.StoreCode}'.");

                    var currencyExists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == line.CurrencyCode);
                    if (!currencyExists)
                        throw new InvalidOperationException($"Invalid Currency Code '{line.CurrencyCode}'.");

                    generalMasters[line.ItemCode] = generalMaster;
                }

                // Pass 2: write everything now that every line has passed.
                foreach (var line in lines)
                {
                    var generalMaster = generalMasters[line.ItemCode];

                    decimal convertedPrice = await _currencyConversionService.ConvertAsync(line.Price, line.CurrencyCode, generalMaster.Currency);
                    decimal qtyInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);

                    generalMaster.QtyInHand = qtyInMasterUnit;
                    generalMaster.Value = Math.Round(qtyInMasterUnit * convertedPrice, 2);
                    _apparelProDbContext.GeneralStockMasters.Update(generalMaster);

                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(new GeneralStockTransaction
                    {
                        TransactionTypeCode = "3A",
                        DocumentNumber = allocatedSanNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = header.StoreCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = line.Price,
                        Currency = line.CurrencyCode,
                    });
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

        public async Task<GeneralSanPrintDetailsServiceModel> GetGeneralSanPrintDetailsAsync(string sanNumber)
        {
            sanNumber = sanNumber.Trim();

            var rows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == sanNumber && t.TransactionTypeCode == "3A")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (rows.Count == 0)
                throw new KeyNotFoundException($"SAN No '{sanNumber}' not found.");

            var first = rows[0];
            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == first.StoreCode);

            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new GeneralSanPrintDetailsServiceModel
            {
                Header = new GeneralSanPrintHeaderServiceModel
                {
                    SanNumber = sanNumber,
                    StoreCode = first.StoreCode,
                    StoreDescription = store?.Description ?? first.StoreCode,
                    TransactionDate = first.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = rows.Select(r => new GeneralSanPrintLineServiceModel
                {
                    ItemCode = r.ItemCode,
                    Description = descriptions.GetValueOrDefault(r.ItemCode, ""),
                    Unit = r.Unit,
                    Quantity = r.Quantity,
                }).ToList(),
            };
        }
    }
}
