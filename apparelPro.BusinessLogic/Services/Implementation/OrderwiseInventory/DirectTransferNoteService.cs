using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_DTN1.PRG (data entry/save) and IN_DTN2.PRG (print) —
    // "DIRECT GOODS TRANSFER NOTE". Mirror image of GoodsTransferNoteService (GTN), but the
    // destination item code is allowed to differ from the source item code - a straight
    // GTN requires the exact same item to already exist on the To side; DTN instead maps
    // the transferred quantity onto whichever item the To Buyer/Order's own material
    // requirement (StyleMaterialCostProfiles) calls for. Basis/Store is shared by both
    // sides, same as legacy's single m_store_cd field.
    //
    // UX deviation from legacy, confirmed by explicit product decision: IN_DTN1.PRG opens a
    // nested grid-in-grid picker (browse the To Order's cost-profile items from inside each
    // From-item entry row). This service instead exposes GetFromStockAsync and
    // GetToOrderItemsAsync as two flat lookups, and the frontend presents them as two
    // separate dropdowns per line - the same simplified-picker pattern already used for
    // every other modernized note type (GIN, GTN, etc.) instead of literally replicating
    // Clipper's nested UI.
    //
    // Architectural deviation from legacy, called out per SKILL.md's Mandatory Architectural
    // Justifications rule: commits the whole note as one atomic DB transaction with
    // UPDLOCK/HOLDLOCK row locks per line, same as every other note type, rather than
    // replicating IN_DTN1.PRG's live ShadowBalance ("shdw_bal") reservation mutation on
    // every line typed into the temp grid before the note is confirmed.
    public class DirectTransferNoteService : IDirectTransferNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public DirectTransferNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<List<DtnFromStockRowServiceModel>> GetFromStockAsync(
            int fromBuyerCode, string fromOrder, int toBuyerCode, string toOrder)
        {
            fromOrder = fromOrder.Trim();
            toOrder = toOrder.Trim();

            // Mirrors legacy: "seek xfbuyer+xforder" / "seek xtbuyer+xtorder" on od_po,
            // "Invalid From/To Buyer/Order.  [F1] - Help"
            var fromBuyerOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == fromBuyerCode && po.Order == fromOrder);
            if (!fromBuyerOrderExists)
                throw new KeyNotFoundException($"Invalid From Buyer/Order (Buyer: {fromBuyerCode}, Order: {fromOrder}).");

            var toBuyerOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == toBuyerCode && po.Order == toOrder);
            if (!toBuyerOrderExists)
                throw new KeyNotFoundException($"Invalid To Buyer/Order (Buyer: {toBuyerCode}, Order: {toOrder}).");

            var fromStockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == fromBuyerCode && s.Order == fromOrder && s.QtyInHand > 0)
                .ToListAsync();

            if (fromStockRows.Count == 0)
                return new List<DtnFromStockRowServiceModel>();

            // Description resolution mirrors the established two-tier convention used by
            // every other note type.
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == fromBuyerCode && p.Order.Trim() == fromOrder)
                .ToListAsync();
            var profileByItemCode = costProfiles.ToDictionary(p => p.ItemCode.Trim(), p => p);

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = fromStockRows.Select(s => DecomposePart(s.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            return fromStockRows.Select(s =>
            {
                var baseItemCode = DecomposePart(s.ItemCode, 2, 4);
                profileByItemCode.TryGetValue(s.ItemCode.Trim(), out var matchingProfile);

                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(baseItemCode, out description);

                return new DtnFromStockRowServiceModel
                {
                    ItemCode = s.ItemCode,
                    StoreCode = s.StoreCode,
                    Unit = s.Unit,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    QtyInHand = s.QtyInHand,
                    MaxTransferableQuantity = s.QtyInHand,
                };
            }).ToList();
        }

        public async Task<List<DtnToItemServiceModel>> GetToOrderItemsAsync(int toBuyerCode, string toOrder)
        {
            toOrder = toOrder.Trim();

            var toBuyerOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == toBuyerCode && po.Order == toOrder);
            if (!toBuyerOrderExists)
                throw new KeyNotFoundException($"Invalid To Buyer/Order (Buyer: {toBuyerCode}, Order: {toOrder}).");

            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == toBuyerCode && p.Order.Trim() == toOrder)
                .ToListAsync();

            return costProfiles
                .GroupBy(p => p.ItemCode.Trim())
                .Select(g => new DtnToItemServiceModel
                {
                    ItemCode = g.Key,
                    Description = g.First().Description?.Trim() ?? "",
                    Unit = g.First().ItemUnit,
                })
                .OrderBy(i => i.ItemCode)
                .ToList();
        }

        public async Task<bool> CommitDirectTransferNoteAsync(
            DtnHeaderServiceModel header,
            List<DtnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Direct Goods Transfer Note cannot be committed without line items.");

            header.FromOrder = header.FromOrder.Trim();
            header.ToOrder = header.ToOrder.Trim();

            // Same guard already established in GoodsTransferNoteService: transferring an
            // order into itself is a pointless no-op in legacy but a hard EF Core identity-map
            // failure here (both stock lookups below would resolve to the same tracked row).
            if (header.FromBuyerCode == header.ToBuyerCode && header.FromOrder == header.ToOrder)
                throw new InvalidOperationException("From and To Buyer/Order must be different for a Direct Goods Transfer Note.");

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var fromBuyerOrderExists = await _apparelProDbContext.PurchaseOrders
                        .AsNoTracking()
                        .AnyAsync(po => po.BuyerCode == header.FromBuyerCode && po.Order == header.FromOrder);
                    if (!fromBuyerOrderExists)
                        throw new InvalidOperationException($"Invalid From Buyer/Order (Buyer: {header.FromBuyerCode}, Order: {header.FromOrder}).");

                    var toBuyerOrderExists = await _apparelProDbContext.PurchaseOrders
                        .AsNoTracking()
                        .AnyAsync(po => po.BuyerCode == header.ToBuyerCode && po.Order == header.ToOrder);
                    if (!toBuyerOrderExists)
                        throw new InvalidOperationException($"Invalid To Buyer/Order (Buyer: {header.ToBuyerCode}, Order: {header.ToOrder}).");

                    string allocatedDtnNumber = await _sharedService.GenerateNextDocumentNumberAsync("DTN");
                    header.DtnNumber = allocatedDtnNumber;

                    foreach (var line in lines)
                    {
                        line.StoreCode = line.StoreCode.Trim().ToUpper();
                        line.FromItemCode = line.FromItemCode.Trim();
                        line.ToItemCode = line.ToItemCode.Trim();
                        line.Unit = line.Unit.Trim().ToUpper();

                        if (line.Quantity <= 0)
                            throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.FromItemCode}'.");

                        var basisExists = await _apparelProDbContext.Basis
                            .AsNoTracking()
                            .AnyAsync(b => b.Code == line.StoreCode);
                        if (!basisExists)
                            throw new InvalidOperationException($"Invalid Basis Code '{line.StoreCode}'.");

                        var unitExists = await _apparelProDbContext.Units
                            .AsNoTracking()
                            .AnyAsync(u => u.Code == line.Unit);
                        if (!unitExists)
                            throw new InvalidOperationException($"Invalid Unit Code '{line.Unit}'.");

                        // Lock and validate the From-side physical stock row.
                        var fromStockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.FromBuyerCode}
                                  AND [Order] = {header.FromOrder}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.FromItemCode}")
                            .FirstOrDefaultAsync();

                        if (fromStockRecord == null)
                            throw new InvalidOperationException($"Item '{line.FromItemCode}' under basis '{line.StoreCode}' does not exist in the From Buyer/Order stock.");

                        decimal requestedInFromStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, fromStockRecord.Unit, line.Quantity);

                        if (requestedInFromStockUnit > fromStockRecord.QtyInHand)
                            throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for Item '{line.FromItemCode}'. Requested: {line.Quantity} {line.Unit}, Available: {await _sharedService.ConvertUnitAsync(fromStockRecord.Unit, line.Unit, fromStockRecord.QtyInHand)} {line.Unit}.");

                        // Lock and validate the To-side physical stock row - the destination
                        // item must already exist under the To Buyer/Order's stock (created
                        // via GRN/PO placement), same requirement as GTN's To-side check.
                        var toStockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.ToBuyerCode}
                                  AND [Order] = {header.ToOrder}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ToItemCode}")
                            .FirstOrDefaultAsync();

                        if (toStockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ToItemCode}' under basis '{line.StoreCode}' does not exist in the To Buyer/Order stock.");

                        decimal requestedInToStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, toStockRecord.Unit, line.Quantity);

                        // Write both legs of the DTN transaction. "8D" (From) and "9D" (To)
                        // are the exact legacy codes (IN_DTN1.PRG: "id with '8D'" / "id with
                        // '9D'") - kept as-is rather than inventing new ones. Each leg's own
                        // ItemCode carries that side's item - the '9D' row's ItemCode is the
                        // mapped destination item, which is the whole difference from GTN's
                        // matching '6T'/'1T' legs (always the same ItemCode on both sides).
                        var transferOutRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedDtnNumber,
                            TransactionType = "8D",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.FromBuyerCode,
                            Order = header.FromOrder,
                            DepartmentCode = string.Empty,
                            StockCode = line.FromItemCode.Substring(0, 2),
                            StoreCode = line.StoreCode,
                            ItemCode = line.FromItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            CounterpartyBuyerCode = header.ToBuyerCode,
                            CounterpartyOrder = header.ToOrder,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(transferOutRow);

                        var transferInRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedDtnNumber,
                            TransactionType = "9D",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.ToBuyerCode,
                            Order = header.ToOrder,
                            DepartmentCode = string.Empty,
                            StockCode = line.ToItemCode.Substring(0, 2),
                            StoreCode = line.StoreCode,
                            ItemCode = line.ToItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            CounterpartyBuyerCode = header.FromBuyerCode,
                            CounterpartyOrder = header.FromOrder,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(transferInRow);

                        fromStockRecord.QtyInHand -= requestedInFromStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(fromStockRecord);

                        toStockRecord.QtyInHand += requestedInToStockUnit;
                        toStockRecord.ToDateReceived += requestedInToStockUnit;
                        toStockRecord.LastDateReceived = header.TransactionDate;
                        _apparelProDbContext.OrderwiseStocks.Update(toStockRecord);

                        // Update order-level master running totals - soft-updated only if the
                        // master row exists, same "if found()" precedent established in every
                        // other note type's service. Reuses GTN's TransferOut/TransferIn
                        // columns since this is the same class of movement, just allowing a
                        // different destination item.
                        var fromMasterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.FromBuyerCode}
                                  AND [Order] = {header.FromOrder}
                                  AND ItemCode = {line.FromItemCode}")
                            .FirstOrDefaultAsync();

                        if (fromMasterRow != null)
                        {
                            decimal requestedInFromMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, fromMasterRow.Unit, line.Quantity);
                            fromMasterRow.TransferOutQuantity += requestedInFromMasterUnit;
                            _apparelProDbContext.OrderwiseStockMasters.Update(fromMasterRow);
                        }

                        var toMasterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.ToBuyerCode}
                                  AND [Order] = {header.ToOrder}
                                  AND ItemCode = {line.ToItemCode}")
                            .FirstOrDefaultAsync();

                        if (toMasterRow != null)
                        {
                            decimal requestedInToMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, toMasterRow.Unit, line.Quantity);
                            toMasterRow.TransferInQuantity += requestedInToMasterUnit;
                            _apparelProDbContext.OrderwiseStockMasters.Update(toMasterRow);
                        }
                    }

                    await _apparelProDbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<DtnPrintDetailsServiceModel> GetDtnPrintDetailsAsync(string dtnNumber)
        {
            dtnNumber = dtnNumber.Trim();

            // Only the "8D" (From) leg is read for the line list - legacy IN_DTN2.PRG's own
            // print only ever shows the From-side item code/description/unit/qty, never the
            // mapped destination item.
            var transactionRows = await _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == dtnNumber && t.TransactionType == "8D")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (transactionRows.Count == 0)
                throw new KeyNotFoundException($"DTN No '{dtnNumber}' not found.");

            var firstRow = transactionRows[0];

            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == firstRow.BuyerCode && p.Order.Trim() == firstRow.Order.Trim())
                .ToListAsync();
            var profileByItemCode = costProfiles.ToDictionary(p => p.ItemCode.Trim(), p => p);

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = transactionRows.Select(t => DecomposePart(t.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var lines = transactionRows.Select(t =>
            {
                var itemCode = t.ItemCode.Trim();
                var baseItemCode = DecomposePart(itemCode, 2, 4);

                profileByItemCode.TryGetValue(itemCode, out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(baseItemCode, out description);

                return new DtnPrintLineServiceModel
                {
                    ItemCode = itemCode,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    Unit = t.Unit,
                    Quantity = t.Quantity,
                    StoreCode = t.StoreCode,
                };
            }).ToList();

            int toBuyerCode = firstRow.CounterpartyBuyerCode ?? 0;
            string toOrder = firstRow.CounterpartyOrder ?? "";

            var buyerCodesToLookUp = new List<int> { firstRow.BuyerCode, toBuyerCode };
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodesToLookUp.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            return new DtnPrintDetailsServiceModel
            {
                Header = new DtnPrintHeaderServiceModel
                {
                    DtnNumber = dtnNumber,
                    FromBuyerCode = firstRow.BuyerCode,
                    FromBuyerName = buyerNames.GetValueOrDefault(firstRow.BuyerCode, firstRow.BuyerCode.ToString()),
                    FromOrder = firstRow.Order,
                    ToBuyerCode = toBuyerCode,
                    ToBuyerName = buyerNames.GetValueOrDefault(toBuyerCode, toBuyerCode.ToString()),
                    ToOrder = toOrder,
                    TransactionDate = firstRow.TransactionDate,
                    // Legacy prints the current system date/time on every print run, not the
                    // original transaction date - same convention as STRN/GIN's own print.
                    PrintedOn = DateTime.Now,
                },
                Lines = lines,
            };
        }
    }
}
