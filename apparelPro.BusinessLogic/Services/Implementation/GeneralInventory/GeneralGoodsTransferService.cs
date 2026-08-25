using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Replicates GI_GGTN1.PRG/GI_GGTN2.PRG - General Inventory's Goods Transfer Note
    // between two General stores. Unlike STRN (which creates a persisted reservation
    // for later GIN fulfillment), a GTN moves stock atomically within one commit, so
    // ShadowBalance is only ever read (for the balance check) here, never written -
    // there's no pending state for this note type to leave behind.
    public class GeneralGoodsTransferService : IGeneralGoodsTransferService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GeneralGoodsTransferService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<bool> CommitGeneralGoodsTransferNoteAsync(
            GeneralGtnHeaderServiceModel header,
            List<GeneralGtnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Goods Transfer Note cannot be committed without line items.");

            header.FromStoreCode = header.FromStoreCode.Trim().ToUpper();
            header.ToStoreCode = header.ToStoreCode.Trim().ToUpper();

            var fromStore = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == header.FromStoreCode);
            if (fromStore == null)
                throw new InvalidOperationException("Invalid From Stores Code.");
            var toStore = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == header.ToStoreCode);
            if (toStore == null)
                throw new InvalidOperationException("Invalid To Stores Code.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string allocatedGtnNumber = await _sharedService.GenerateNextDocumentNumberAsync("GGTN");

                var fromMasters = new Dictionary<string, GeneralStockMaster>();
                var toMasters = new Dictionary<string, GeneralStockMaster>();

                // Pass 1: lock and validate every line before writing anything.
                foreach (var line in lines)
                {
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();

                    var fromMaster = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {header.FromStoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();
                    if (fromMaster == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' not found in [Transfer From Stores].");

                    var toMaster = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {header.ToStoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();
                    if (toMaster == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' not found in [Transfer To Stores].");

                    decimal transferQtyInFromUnit = await _sharedService.ConvertUnitAsync(line.Unit, fromMaster.Unit, line.Quantity);
                    if (transferQtyInFromUnit > fromMaster.QtyInHand - fromMaster.ShadowBalance)
                        throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for item '{line.ItemCode}'.");

                    fromMasters[line.ItemCode] = fromMaster;
                    toMasters[line.ItemCode] = toMaster;
                }

                // Pass 2: write everything now that every line has passed.
                foreach (var line in lines)
                {
                    var fromMaster = fromMasters[line.ItemCode];
                    var toMaster = toMasters[line.ItemCode];

                    decimal qtyInFromUnit = await _sharedService.ConvertUnitAsync(line.Unit, fromMaster.Unit, line.Quantity);
                    decimal qtyInToUnit = await _sharedService.ConvertUnitAsync(line.Unit, toMaster.Unit, line.Quantity);
                    decimal avgPrice = fromMaster.QtyInHand == 0 ? 0 : Math.Round(fromMaster.Value / fromMaster.QtyInHand, 4);

                    fromMaster.QtyInHand -= qtyInFromUnit;
                    fromMaster.Value -= Math.Round(qtyInFromUnit * avgPrice, 2);
                    _apparelProDbContext.GeneralStockMasters.Update(fromMaster);

                    toMaster.QtyInHand += qtyInToUnit;
                    toMaster.Value += Math.Round(qtyInToUnit * avgPrice, 2);
                    _apparelProDbContext.GeneralStockMasters.Update(toMaster);

                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(new GeneralStockTransaction
                    {
                        TransactionTypeCode = "6TG",
                        DocumentNumber = allocatedGtnNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = header.FromStoreCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = avgPrice,
                    });
                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(new GeneralStockTransaction
                    {
                        TransactionTypeCode = "1TG",
                        DocumentNumber = allocatedGtnNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = header.ToStoreCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = avgPrice,
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

        public async Task<GeneralGtnPrintDetailsServiceModel> GetGeneralGtnPrintDetailsAsync(string gtnNumber)
        {
            gtnNumber = gtnNumber.Trim();

            var outgoingRow = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.DocumentNumber == gtnNumber && t.TransactionTypeCode == "6TG");
            if (outgoingRow == null)
                throw new KeyNotFoundException($"GTN No '{gtnNumber}' not found.");

            var incomingRows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == gtnNumber && t.TransactionTypeCode == "1TG")
                .OrderBy(t => t.Id)
                .ToListAsync();

            var fromStore = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == outgoingRow.StoreCode);
            var toStore = incomingRows.Count > 0
                ? await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == incomingRows[0].StoreCode)
                : null;

            var itemCodes = incomingRows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new GeneralGtnPrintDetailsServiceModel
            {
                Header = new GeneralGtnPrintHeaderServiceModel
                {
                    GtnNumber = gtnNumber,
                    FromStoreCode = outgoingRow.StoreCode,
                    FromStoreDescription = fromStore?.Description ?? outgoingRow.StoreCode,
                    ToStoreCode = incomingRows.Count > 0 ? incomingRows[0].StoreCode : "",
                    ToStoreDescription = toStore?.Description ?? (incomingRows.Count > 0 ? incomingRows[0].StoreCode : ""),
                    TransactionDate = outgoingRow.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = incomingRows.Select(r => new GeneralGtnPrintLineServiceModel
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
