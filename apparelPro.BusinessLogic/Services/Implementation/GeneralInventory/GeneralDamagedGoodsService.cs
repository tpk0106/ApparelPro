using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_DGN1.PRG/GI_DGN2.PRG - "DAMAGED GOODS NOTE
    // (General)". Writes off a quantity of an item at a General store as damaged -
    // decrements QtyInHand/Value the same way a GIN does, but tracks the quantity under
    // DamagedQuantity instead of issuing it to a Department. No ShadowBalance reservation
    // during entry (atomic validate-then-commit, same convention as every other General
    // Inventory note type here), no gi_monst writes (aggregate live).
    public class GeneralDamagedGoodsService : IGeneralDamagedGoodsService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GeneralDamagedGoodsService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<bool> CommitGeneralDamagedGoodsNoteAsync(
            GeneralDgnHeaderServiceModel header,
            List<GeneralDgnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Damaged Goods Note cannot be committed without line items.");

            header.StoreCode = header.StoreCode.Trim().ToUpper();

            var storeExists = await _apparelProDbContext.GeneralStores.AsNoTracking().AnyAsync(s => s.Code == header.StoreCode);
            if (!storeExists)
                throw new InvalidOperationException($"Invalid Stores Code '{header.StoreCode}'.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string allocatedDgnNumber = await _sharedService.GenerateNextDocumentNumberAsync("GDGN");

                var generalMasters = new Dictionary<string, GeneralStockMaster>();

                // Pass 1: lock and validate every line before writing anything.
                foreach (var line in lines)
                {
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                    var generalMaster = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {header.StoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();
                    if (generalMaster == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' not found in Stores '{header.StoreCode}'.");

                    decimal requestedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);
                    if (requestedInMasterUnit > generalMaster.QtyInHand - generalMaster.ShadowBalance)
                        throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for Item '{line.ItemCode}'.");

                    generalMasters[line.ItemCode] = generalMaster;
                }

                // Pass 2: write everything now that every line has passed.
                foreach (var line in lines)
                {
                    var generalMaster = generalMasters[line.ItemCode];

                    decimal avgPrice = generalMaster.QtyInHand == 0 ? 0 : Math.Round(generalMaster.Value / generalMaster.QtyInHand, 2);
                    decimal qtyInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);

                    generalMaster.QtyInHand -= qtyInMasterUnit;
                    generalMaster.Value -= Math.Round(qtyInMasterUnit * avgPrice, 2);
                    generalMaster.DamagedQuantity += qtyInMasterUnit;
                    _apparelProDbContext.GeneralStockMasters.Update(generalMaster);

                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(new GeneralStockTransaction
                    {
                        TransactionTypeCode = "6D",
                        DocumentNumber = allocatedDgnNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = header.StoreCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = avgPrice,
                        Currency = generalMaster.Currency,
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

        public async Task<GeneralDgnPrintDetailsServiceModel> GetGeneralDgnPrintDetailsAsync(string dgnNumber)
        {
            dgnNumber = dgnNumber.Trim();

            var rows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == dgnNumber && t.TransactionTypeCode == "6D")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (rows.Count == 0)
                throw new KeyNotFoundException($"DGN No '{dgnNumber}' not found.");

            var first = rows[0];
            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == first.StoreCode);

            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new GeneralDgnPrintDetailsServiceModel
            {
                Header = new GeneralDgnPrintHeaderServiceModel
                {
                    DgnNumber = dgnNumber,
                    StoreCode = first.StoreCode,
                    StoreDescription = store?.Description ?? first.StoreCode,
                    TransactionDate = first.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = rows.Select(r => new GeneralDgnPrintLineServiceModel
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
