using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_GTN1.PRG (data entry/save, the production-wired version —
    // IN_MENU.PRG calls chk_pass('in_gtn1') directly, unlike GIN/AIN/ARN which route through a
    // '*3' variant) and IN_GTN2.PRG (print) — "GOODS TRANSFER NOTE". Moves a quantity of an
    // item that already exists under one Buyer/Order's stock into the SAME item code under a
    // different Buyer/Order's stock (same Basis/StoreCode both sides). NOTE: a second legacy
    // file, IN_GTN3.PRG, implements a richer variant that also lets the destination item code
    // differ from the source — but it is never referenced by IN_MENU.PRG (no "do in_gtn3"
    // override exists the way GIN/AIN/ARN have one), so it was not wired into production and is
    // deliberately NOT the basis for this migration, per the Zero-Assumption Boundary Rule
    // (only IN_GTN1.PRG's behavior is confirmed live).
    //
    // Architectural deviation from legacy, called out explicitly per SKILL.md's Mandatory
    // Architectural Justifications rule: IN_GTN1.PRG reserves stock via a live ShadowBalance
    // ("shdw_bal") mutation on every line typed into the temp grid, *before* the note is ever
    // confirmed — a workaround for Clipper's separate entry-then-confirm-loop UI. This
    // codebase's GIN/GRN/RTN/STRN services don't replicate that; each commits a whole note as
    // one atomic DB transaction with UPDLOCK/HOLDLOCK row locks taken per line, which gives the
    // same concurrency safety without a persisted reservation column. GTN follows that same
    // established convention instead of reintroducing ShadowBalance semantics.
    public class GoodsTransferNoteService : IGoodsTransferNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;
        private readonly IUnitConversionService _unitConversionService;

        public GoodsTransferNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService,
            IUnitConversionService unitConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
            _unitConversionService = unitConversionService;
        }

        public async Task<List<GtnTransferableStockRowServiceModel>> GetTransferableStockAsync(
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

            // Mirrors legacy's "copy whil buyer+order = xfbuyer+xforder to temp1" snapshot,
            // narrowed to rows that actually have something to transfer.
            var fromStockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == fromBuyerCode && s.Order == fromOrder && s.QtyInHand > 0)
                .ToListAsync();

            if (fromStockRows.Count == 0)
                return new List<GtnTransferableStockRowServiceModel>();

            // Mirrors legacy's second validation ("Item Code not found in [To Buyer/Order]") —
            // pre-filtered here into the picker instead of surfaced as a late commit error.
            var toStockKeys = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == toBuyerCode && s.Order == toOrder)
                .Select(s => new { s.StoreCode, s.ItemCode })
                .ToListAsync();
            var toStockKeySet = toStockKeys.Select(k => (k.StoreCode, k.ItemCode)).ToHashSet();

            var transferableRows = fromStockRows
                .Where(s => toStockKeySet.Contains((s.StoreCode, s.ItemCode)))
                .ToList();

            if (transferableRows.Count == 0)
                return new List<GtnTransferableStockRowServiceModel>();

            // Description resolution mirrors StoresRequisitionService/GoodsReturnNoteService's
            // established two-tier convention: StyleMaterialCostProfiles (od_sacc2) is the
            // authoritative, style/feature-specific source, keyed by the full 22-char composite
            // ItemCode; StockItems is a plain ItemCode -> Description catalog keyed by just the
            // 4-char base item code (StockCode(2) stripped off), used only as a fallback.
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == fromBuyerCode && p.Order.Trim() == fromOrder)
                .ToListAsync();
            var profileByItemCode = costProfiles.ToDictionary(p => p.ItemCode.Trim(), p => p);

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = transferableRows.Select(s => DecomposePart(s.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            return transferableRows.Select(s =>
            {
                var baseItemCode = DecomposePart(s.ItemCode, 2, 4);
                profileByItemCode.TryGetValue(s.ItemCode.Trim(), out var matchingProfile);

                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(baseItemCode, out description);

                return new GtnTransferableStockRowServiceModel
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

        public async Task<bool> CommitGoodsTransferNoteAsync(
            GtnHeaderServiceModel header,
            List<GtnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Goods Transfer Note cannot be committed without line items.");

            header.FromOrder = header.FromOrder.Trim();
            header.ToOrder = header.ToOrder.Trim();

            // Pre-Migration Defect Audit finding: legacy never guards against transferring an
            // order into itself. In Clipper that's merely a pointless no-op; under EF Core's
            // identity map it's a hard failure — the From and To stock lookups below would
            // resolve to the exact same physical row, and tracking two separate query results
            // against the same primary key throws InvalidOperationException. Blocking it
            // upfront with a clear message is strictly safer than either replicating the
            // legacy no-op or letting the ORM fail with a confusing tracking-conflict error.
            if (header.FromBuyerCode == header.ToBuyerCode && header.FromOrder == header.ToOrder)
                throw new InvalidOperationException("From and To Buyer/Order must be different for a Goods Transfer Note.");

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Re-validate both Buyer/Orders — never trust a client echo, even though
                    // the frontend will have already called GetTransferableStockAsync.
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

                    // 2. Thread-safe allocation of the next GTN number.
                    string allocatedGtnNumber = await _sharedService.GenerateNextDocumentNumberAsync("GTN");
                    header.GtnNumber = allocatedGtnNumber;

                    foreach (var line in lines)
                    {
                        line.StoreCode = line.StoreCode.Trim().ToUpper();
                        line.ItemCode = line.ItemCode.Trim();
                        line.Unit = line.Unit.Trim().ToUpper();

                        if (line.Quantity <= 0)
                            throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                        // 3. Validate Basis code — mirrors legacy "seek m_store_cd" on od_bref,
                        // "Invalid Basis Code."
                        var basisExists = await _apparelProDbContext.Basis
                            .AsNoTracking()
                            .AnyAsync(b => b.Code == line.StoreCode);
                        if (!basisExists)
                            throw new InvalidOperationException($"Invalid Basis Code '{line.StoreCode}'.");

                        // 4. Validate Unit code — mirrors legacy "seek m_unit" on od_uref,
                        // "Invalid Unit Code."
                        var unitExists = await _apparelProDbContext.Units
                            .AsNoTracking()
                            .AnyAsync(u => u.Code == line.Unit);
                        if (!unitExists)
                            throw new InvalidOperationException($"Invalid Unit Code '{line.Unit}'.");

                        // 5. Lock and validate the From-side physical stock row — mirrors legacy
                        // "seek xfbuyer+xforder+m_store_cd+m_item_cd" on in_stock, "Item Code
                        // not found in [From Buyer/Order]." Same UPDLOCK/HOLDLOCK reasoning as
                        // GoodsIssueService/GoodsReturnNoteService: another concurrent
                        // GTN/GIN/RTN could be mutating this exact row's QtyInHand right now.
                        var fromStockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.FromBuyerCode}
                                  AND [Order] = {header.FromOrder}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (fromStockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under basis '{line.StoreCode}' does not exist in the From Buyer/Order stock.");

                        decimal requestedInFromStockUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, fromStockRecord.Unit, line.Quantity);

                        // 6. Hard block — mirrors legacy "m_qty > convert(unit,m_unit,(qty_in_hd-shdw_bal))",
                        // "Attempt to Exceed Balance Quantity." (ShadowBalance omitted — see class-level note.)
                        if (requestedInFromStockUnit > fromStockRecord.QtyInHand)
                            throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Available: {await _unitConversionService.ConvertUnitAsync(fromStockRecord.Unit, line.Unit, fromStockRecord.QtyInHand)} {line.Unit}.");

                        // 7. Lock and validate the To-side physical stock row — mirrors legacy
                        // "seek xtbuyer+xtorder+m_store_cd+m_item_cd" on in_stock, "Item Code
                        // not found in [To Buyer/Order]." The destination must already carry
                        // this item/basis combination; a transfer cannot create a brand-new
                        // stock line on the receiving order.
                        var toStockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.ToBuyerCode}
                                  AND [Order] = {header.ToOrder}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (toStockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under basis '{line.StoreCode}' does not exist in the To Buyer/Order stock.");

                        decimal requestedInToStockUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, toStockRecord.Unit, line.Quantity);

                        // 8. Write both legs of the GTN transaction. "6T" (Transfer-Out) and
                        // "1T" (Transfer-In) are the exact legacy codes (IN_GTN1.PRG: "repl ...
                        // id with '6T'" / "id with '1T'") — kept as-is rather than inventing new
                        // ones, same rigor already applied to RTN's "2R" and STRN's "0S".
                        var transferOutRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedGtnNumber,
                            TransactionType = "6T",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.FromBuyerCode,
                            Order = header.FromOrder,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Substring(0, 2),
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            CounterpartyBuyerCode = header.ToBuyerCode,
                            CounterpartyOrder = header.ToOrder,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(transferOutRow);

                        var transferInRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedGtnNumber,
                            TransactionType = "1T",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.ToBuyerCode,
                            Order = header.ToOrder,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Substring(0, 2),
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            CounterpartyBuyerCode = header.FromBuyerCode,
                            CounterpartyOrder = header.FromOrder,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(transferInRow);

                        // 9. Update physical stock ledgers — mirrors legacy's
                        // "qty_in_hd with qty_in_hd-mx_qty" (From) / "qty_in_hd with
                        // qty_in_hd+mx_qty, to_dt_rec with to_dt_rec+mx_qty" (To).
                        fromStockRecord.QtyInHand -= requestedInFromStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(fromStockRecord);

                        toStockRecord.QtyInHand += requestedInToStockUnit;
                        toStockRecord.ToDateReceived += requestedInToStockUnit;
                        toStockRecord.LastDateReceived = header.TransactionDate;
                        _apparelProDbContext.OrderwiseStocks.Update(toStockRecord);

                        // 10. Update order-level master running totals — mirrors legacy's
                        // "trou_qty with trou_qty+mx_qty" (From) / "trin_qty with
                        // trin_qty+mx_qty" (To). Soft-updated only if the master row exists,
                        // same "if found()" precedent already established in
                        // GoodsIssueService/GoodsReturnNoteService.
                        var fromMasterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.FromBuyerCode}
                                  AND [Order] = {header.FromOrder}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (fromMasterRow != null)
                        {
                            decimal requestedInFromMasterUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, fromMasterRow.Unit, line.Quantity);
                            fromMasterRow.TransferOutQuantity += requestedInFromMasterUnit;
                            _apparelProDbContext.OrderwiseStockMasters.Update(fromMasterRow);
                        }

                        var toMasterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.ToBuyerCode}
                                  AND [Order] = {header.ToOrder}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (toMasterRow != null)
                        {
                            decimal requestedInToMasterUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, toMasterRow.Unit, line.Quantity);
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
    }
}
