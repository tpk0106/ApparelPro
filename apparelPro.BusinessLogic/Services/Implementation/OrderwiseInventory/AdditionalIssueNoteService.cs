using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_AIN1.PRG (data entry/save) and IN_AIN3.PRG (the later,
    // corrected revision of the same program - see below) plus IN_AIN2.PRG (print).
    // "ADDITIONAL ISSUE NOTE" - issues extra raw material against a Buyer/Order to a
    // Sub-Contractor for a specific Additional Process (e.g. embroidery, printing) already
    // assigned to that order via GarmentAdditionalCost (legacy od_aitm).
    //
    // Built from IN_AIN3.PRG, not IN_AIN1.PRG: IN_AIN3 is a later revision of the same
    // program (confirmed by reading both in full) that fixes a real bug in IN_AIN1 - on
    // "Cancel All Entries" (case lastkey()=27, declined), IN_AIN1 returns without undoing
    // the shdw_bal reservations it had accumulated while the operator was building the
    // entry grid, permanently inflating ShadowBalance. IN_AIN3 adds the missing rollback
    // loop before returning. IN_AIN3 also swaps the balance-check formula from
    // "qty_in_hd-shdw_bal" (physical stock) to "ord_qty-shdw_bal-to_dt_iss" (remaining
    // order-level allocation headroom) - the more deliberate, order-aware version of the
    // check, so that's what this service implements below.
    //
    // Architectural deviation from legacy, called out per SKILL.md's Mandatory
    // Architectural Justifications rule: legacy is a two-phase flow - items are added to a
    // live temp grid (each add immediately bumps OrderwiseStock.ShadowBalance as a soft
    // reservation), then only on final confirm does it additionally decrement QtyInHand/
    // bump ToDateIssued and net the ShadowBalance reservation back out again. A stateless
    // REST commit has no equivalent "live editing session" to reserve against, so this
    // service collapses both phases into the single atomic commit below (matching how
    // GIN/STRN/SAN/DGN/RTN/GTN/SRN all already do this) - the net effect on
    // OrderwiseStock after a successful commit is identical to legacy's end state
    // (ShadowBalance untouched, QtyInHand/ToDateIssued moved by the issued amount).
    //
    // Zero-Assumption gaps resolved before writing this service (2026-08-09, user-
    // approved): (1) Sub Contractor (od_scref) had no entity anywhere in the app - built
    // a minimal SubContractor reference table (Code+Name) as a prerequisite, see that
    // entity's own comment. (2) legacy's od_aitm ("does this order have this Additional
    // Process assigned") check already has a modern equivalent - GarmentAdditionalCost,
    // whose own class comment confirms "Replicates od_aitm.dbf" - no new table needed, just
    // an existence check scoped down to Buyer+Order+AdditionalCostCode (legacy's od_aitm2
    // index ignores Type/Style, so this mirrors that exactly rather than over-narrowing).
    // (3) legacy's in_stmst.issd_qty is a single column shared with GIN - this system gives
    // every note type its own dedicated running-total column instead (matching
    // IssuedQuantity/ReceivedQuantity/ReturnedQuantity/etc. already on OrderwiseStockMaster),
    // so AIN gets its own AdditionalIssuedQuantity column rather than commingling into
    // IssuedQuantity, per explicit user decision.
    public class AdditionalIssueNoteService : IAdditionalIssueNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public AdditionalIssueNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<List<AinIssuableStockRowServiceModel>> GetIssuableStockByBuyerOrderAsync(
            int buyerCode, string order)
        {
            order = order.Trim();

            // Mirrors legacy: "seek xbuyer+xorder" on od_po, "Invalid Buyer/Order  [F1] - Help"
            var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!buyerOrderExists)
                throw new KeyNotFoundException($"Invalid Buyer/Order (Buyer: {buyerCode}, Order: {order}).");

            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();

            if (stockRows.Count == 0)
                return new List<AinIssuableStockRowServiceModel>();

            // Description AND eligibility source is StyleMaterialCostProfiles (od_sacc2) -
            // legacy hard-blocks any item without a matching profile row ("No Material
            // Consumptions made for Item"), so unlike SAN/DGN's two-tier description
            // fallback, an item with no profile row is left out of the picker entirely
            // rather than shown with a StockItems-catalog fallback description.
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                .ToListAsync();
            var profileByItemCode = costProfiles
                .GroupBy(p => p.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            var resultList = new List<AinIssuableStockRowServiceModel>();
            foreach (var stock in stockRows)
            {
                if (!profileByItemCode.TryGetValue(stock.ItemCode.Trim(), out var profile))
                    continue;

                resultList.Add(new AinIssuableStockRowServiceModel
                {
                    ItemCode = stock.ItemCode,
                    StoreCode = stock.StoreCode,
                    Unit = stock.Unit,
                    Description = profile.Description,
                    OrderedQuantity = stock.OrderedQuantity,
                    ShadowBalance = stock.ShadowBalance,
                    ToDateIssued = stock.ToDateIssued,
                    QtyInHand = stock.QtyInHand,
                    AvailableForIssue = stock.OrderedQuantity - stock.ShadowBalance - stock.ToDateIssued,
                });
            }

            return resultList.OrderBy(r => r.ItemCode).ToList();
        }

        public async Task<bool> CommitAdditionalIssueNoteAsync(
            AinHeaderServiceModel header,
            List<AinLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Additional Issue Note cannot be committed without line items.");

            header.Order = header.Order.Trim();
            header.SubContractorCode = header.SubContractorCode.Trim().ToUpper();
            header.AdditionalProcessCode = header.AdditionalProcessCode.Trim().ToUpper();

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Re-validate the Buyer/Order - never trust a client echo.
                    var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                        .AsNoTracking()
                        .AnyAsync(po => po.BuyerCode == header.BuyerCode && po.Order == header.Order);
                    if (!buyerOrderExists)
                        throw new InvalidOperationException($"Invalid Buyer/Order (Buyer: {header.BuyerCode}, Order: {header.Order}).");

                    // 2. Validate Sub-Contractor's Code - mirrors legacy "seek m_subcnt" on od_scref.
                    var subContractorExists = await _apparelProDbContext.SubContractors
                        .AsNoTracking()
                        .AnyAsync(s => s.Code == header.SubContractorCode);
                    if (!subContractorExists)
                        throw new InvalidOperationException($"Invalid Sub-Contractor Code '{header.SubContractorCode}'.");

                    // 3. Validate Additional Process Code - mirrors legacy "seek m_acost" on od_acost.
                    var additionalProcessExists = await _apparelProDbContext.AdditionalCosts
                        .AsNoTracking()
                        .AnyAsync(a => a.Code == header.AdditionalProcessCode);
                    if (!additionalProcessExists)
                        throw new InvalidOperationException($"Invalid Additional Process Code '{header.AdditionalProcessCode}'.");

                    // 4. Confirm this Buyer/Order actually has that Additional Process assigned -
                    // mirrors legacy "seek xbuyer+xorder+m_acost" on od_aitm ("No such Additional
                    // Process for Given Buyer/Order."). GarmentAdditionalCost is the modern
                    // equivalent of od_aitm (see its own class comment) - legacy's od_aitm2 index
                    // only keys on Buyer+Order+Process (no Type/Style), so this check is
                    // deliberately scoped the same way rather than narrowed further.
                    var additionalProcessAssigned = await _apparelProDbContext.GarmentAdditionalCosts
                        .AsNoTracking()
                        .AnyAsync(g => g.BuyerCode == header.BuyerCode && g.Order == header.Order
                                    && g.AdditionalCostCode == header.AdditionalProcessCode);
                    if (!additionalProcessAssigned)
                        throw new InvalidOperationException($"No Additional Process '{header.AdditionalProcessCode}' assigned to Buyer/Order '{header.BuyerCode}/{header.Order}'.");

                    // 5. Thread-safe allocation of the next AIN number.
                    string allocatedAinNumber = await _sharedService.GenerateNextDocumentNumberAsync("AIN");
                    header.AinNumber = allocatedAinNumber;

                    foreach (var line in lines)
                    {
                        line.StoreCode = line.StoreCode.Trim().ToUpper();
                        line.ItemCode = line.ItemCode.Trim();
                        line.Unit = line.Unit.Trim().ToUpper();

                        // Legacy: "valid m_qty > 0".
                        if (line.Quantity <= 0)
                            throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                        // 6. Lock and validate the physical stock row - mirrors legacy
                        // "seek xbuyer+xorder+m_store_cd+m_item_cd" on in_stock,
                        // "Item not found in Stock Master File."
                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (stockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under basis '{line.StoreCode}' does not exist in the stock master file.");

                        // 7. Material Consumption profile must exist - mirrors legacy
                        // "seek xbuyer+xorder+m_item_cd" on od_sacc2, "No Material Consumptions
                        // made for Item".
                        var hasCostProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                            .AsNoTracking()
                            .AnyAsync(p => p.BuyerCode == header.BuyerCode && p.Order.Trim() == header.Order && p.ItemCode == line.ItemCode);
                        if (!hasCostProfile)
                            throw new InvalidOperationException($"No Material Consumption profile found for Item '{line.ItemCode}' - cannot raise an Additional Issue against it.");

                        // 8. Unit compatibility + conversion - mirrors legacy's chk_unit()/convert().
                        decimal requestedInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.Quantity);

                        // 9a. Order-level allocation headroom check - mirrors IN_AIN3.PRG's
                        // "ord_qty-shdw_bal-to_dt_iss < m_qty" -> "Entered Quantity is less than
                        // Qty. in Hand" (legacy's own error text was left stale from the older
                        // IN_AIN1 formula when the check was swapped - reworded here to describe
                        // what's actually being checked).
                        decimal availableForIssue = stockRecord.OrderedQuantity - stockRecord.ShadowBalance - stockRecord.ToDateIssued;
                        if (requestedInStockUnit > availableForIssue)
                            throw new InvalidOperationException($"Entered Quantity exceeds the remaining Order allocation for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Available: {await _sharedService.ConvertUnitAsync(stockRecord.Unit, line.Unit, availableForIssue)} {line.Unit}.");

                        // 9b. Physical availability guard - an addition beyond legacy (which
                        // never separately checked qty_in_hd in the IN_AIN3 branch, only the
                        // allocation headroom above), added so a physical QtyInHand shortfall
                        // can never be masked and driven negative by this note.
                        if (requestedInStockUnit > stockRecord.QtyInHand)
                            throw new InvalidOperationException($"Cannot issue more than Quantity In Hand for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Qty In Hand: {await _sharedService.ConvertUnitAsync(stockRecord.Unit, line.Unit, stockRecord.QtyInHand)} {line.Unit}.");

                        // 10. Write the AIN transaction row. "4X" is the exact legacy code
                        // (IN_AIN1.PRG/IN_AIN3.PRG: "id with '4X'"). DepartmentCode has no
                        // legacy AIN equivalent - same string.Empty convention already used by
                        // DGN/SAN/SRN for note types without a department concept.
                        var ainRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedAinNumber,
                            TransactionType = "4X",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Length >= 2 ? line.ItemCode.Substring(0, 2) : line.ItemCode,
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            SubContractorCode = header.SubContractorCode,
                            AdditionalProcessCode = header.AdditionalProcessCode,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(ainRow);

                        // 11. Physical issue - mirrors legacy's "repl qty_in_hd with qty_in_hd-mx_qty,
                        // to_dt_iss with to_dt_iss+mx_qty, l_dt_iss with d_toc(xdate)". ShadowBalance
                        // is deliberately left untouched - see the class-level architecture note.
                        stockRecord.QtyInHand -= requestedInStockUnit;
                        stockRecord.ToDateIssued += requestedInStockUnit;
                        stockRecord.LastDateIssued = header.TransactionDate;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);

                        // 12. Update order-level master running total - mirrors legacy's
                        // "repl issd_qty with issd_qty+mx_qty, bal_qty with bal_qty-mx_qty" on
                        // in_stmst, but into AdditionalIssuedQuantity (this note type's own
                        // dedicated column) rather than IssuedQuantity - see class comment.
                        // bal_qty has no direct write here: Balance is always derived, never
                        // persisted, same convention as every other note type.
                        var masterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (masterRow != null)
                        {
                            decimal requestedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, masterRow.Unit, line.Quantity);
                            masterRow.AdditionalIssuedQuantity += requestedInMasterUnit;
                            _apparelProDbContext.OrderwiseStockMasters.Update(masterRow);
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

        // Modern equivalent of legacy IN_AIN2.PRG - "seek m_docno+'4X'" then walks
        // in_sttr while docno+id matches, printing Item/Description/Unit/Qty/Store.
        public async Task<AinPrintDetailsServiceModel> GetAinPrintDetailsAsync(string ainNumber)
        {
            ainNumber = ainNumber.Trim();

            var transactionRows = await _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == ainNumber && t.TransactionType == "4X")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (transactionRows.Count == 0)
                throw new KeyNotFoundException($"AIN No '{ainNumber}' not found.");

            var firstRow = transactionRows[0];

            // Same two-tier description lookup as StoresRequisitionService.GetStrnPrintDetailsAsync
            // (cost profile primary, StockItems catalog fallback) - legacy IN_AIN2.PRG only
            // ever reads od_sacc2 with no fallback, so this is a deliberate improvement.
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == firstRow.BuyerCode && p.Order.Trim() == firstRow.Order)
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
                profileByItemCode.TryGetValue(t.ItemCode.Trim(), out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(DecomposePart(t.ItemCode, 2, 4), out description);

                return new AinPrintLineServiceModel
                {
                    ItemCode = t.ItemCode.Trim(),
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    Unit = t.Unit,
                    Quantity = t.Quantity,
                    StoreCode = t.StoreCode,
                };
            }).ToList();

            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == firstRow.BuyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync();

            return new AinPrintDetailsServiceModel
            {
                Header = new AinPrintHeaderServiceModel
                {
                    AinNumber = ainNumber,
                    BuyerCode = firstRow.BuyerCode,
                    BuyerName = !string.IsNullOrWhiteSpace(buyerName) ? buyerName : firstRow.BuyerCode.ToString(),
                    Order = firstRow.Order,
                    SubContractorCode = firstRow.SubContractorCode ?? "",
                    AdditionalProcessCode = firstRow.AdditionalProcessCode ?? "",
                    TransactionDate = firstRow.TransactionDate,
                    PrintedOn = DateTime.Now,
                },
                Lines = lines,
            };
        }
    }
}
