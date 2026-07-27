using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    public class GoodsIssueService : IGoodsIssueService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;
        private readonly IUnitConversionService _unitConversionService;

        public GoodsIssueService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService,
            IUnitConversionService unitConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
            _unitConversionService = unitConversionService;
        }

        public async Task<GinStrnLookupResultServiceModel> GetIssuableStrnLinesAsync(string strnNumber)
        {
            strnNumber = strnNumber.Trim().ToUpper();

            // 1. Pull every STRN line for this document in one query (no per-line round trips).
            var strnLines = await _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == strnNumber && t.TransactionType == "0S")
                .ToListAsync();

            if (!strnLines.Any())
                throw new KeyNotFoundException($"Stores Requisition Note '{strnNumber}' was not found.");

            var outstandingLines = strnLines.Where(l => l.BalanceToReceive > 0).ToList();
            if (!outstandingLines.Any())
                throw new InvalidOperationException($"Stores Requisition Note '{strnNumber}' has already been fully issued.");

            // 2. Batch-load live stock context for every outstanding line's (store, item) pair —
            // same N+1 avoidance pattern already established in StoresRequisitionService.
            var buyerCode = strnLines[0].BuyerCode;
            var order = strnLines[0].Order;

            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();
            var stockByStoreItem = stockRows.ToDictionary(s => (s.StoreCode, s.ItemCode));

            // 3. Resolve item descriptions with the same two-tier lookup convention already
            // established for STRN/RTN: StyleMaterialCostProfiles (od_sacc2) is the authoritative,
            // style-specific source; StockItems is a generic ItemCode -> Description catalog fallback
            // for anything without a matching cost profile row.
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                .ToListAsync();
            var profileByItemCode = costProfiles.ToDictionary(p => p.ItemCode.Trim(), p => p);

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = outstandingLines
                .Select(l => DecomposePart(l.ItemCode, 2, 4))
                .Distinct()
                .ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var resultLines = outstandingLines.Select(l =>
            {
                stockByStoreItem.TryGetValue((l.StoreCode, l.ItemCode), out var stock);

                profileByItemCode.TryGetValue(l.ItemCode.Trim(), out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(DecomposePart(l.ItemCode, 2, 4), out description);

                return new GinIssuableStrnLineServiceModel
                {
                    StockCode = l.StockCode,
                    ItemCode = l.ItemCode,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    StoreCode = l.StoreCode,
                    Unit = l.Unit,
                    BalanceToReceive = l.BalanceToReceive,
                    QtyInHand = stock?.QtyInHand ?? 0,
                    StrnBalance = stock?.StrnBalance ?? 0
                };
            }).ToList();

            return new GinStrnLookupResultServiceModel
            {
                BuyerCode = buyerCode,
                Order = order,
                DepartmentCode = strnLines[0].DepartmentCode,
                Lines = resultLines
            };
        }

        public async Task<bool> CommitGoodsIssueNoteAsync(
            GinHeaderServiceModel header,
            List<GinLineItemServiceModel> lines,
            string username,
            bool isManagerOverrideAuthorized,
            bool overrideExactConsumptionCheck)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Goods Issue Note cannot be committed without line items.");

            header.SourceStrnNumber = header.SourceStrnNumber.Trim().ToUpper();

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Re-derive header context from the STRN itself — never trust a client echo.
                    var strnHeaderRow = await _apparelProDbContext.OrderwiseStockTransactions
                        .AsNoTracking()
                        .Where(t => t.DocumentNumber == header.SourceStrnNumber && t.TransactionType == "0S")
                        .FirstOrDefaultAsync();

                    if (strnHeaderRow == null)
                        throw new InvalidOperationException($"Stores Requisition Note '{header.SourceStrnNumber}' was not found.");

                    header.BuyerCode = strnHeaderRow.BuyerCode;
                    header.Order = strnHeaderRow.Order;
                    header.DepartmentCode = strnHeaderRow.DepartmentCode;

                    // 2. Thread-safe allocation of the next GIN number.
                    string allocatedGinNumber = await _sharedService.GenerateNextDocumentNumberAsync("GIN");

                    foreach (var line in lines)
                    {
                        line.StockCode = line.StockCode.Trim();
                        line.ItemCode = line.ItemCode.Trim();
                        line.StoreCode = line.StoreCode.Trim().ToUpper();
                        line.Unit = line.Unit.Trim().ToUpper();

                        // 3. Lock and validate the matching STRN line — hard block if this would
                        // exceed what's still outstanding on that specific STRN line.
                        var strnLine = await _apparelProDbContext.OrderwiseStockTransactions
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockTransactions WITH (UPDLOCK, HOLDLOCK)
                                WHERE DocumentNumber = {header.SourceStrnNumber}
                                  AND TransactionType = '0S'
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (strnLine == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under store '{line.StoreCode}' was not requisitioned on STRN '{header.SourceStrnNumber}'.");

                        decimal requestedInStrnUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, strnLine.Unit, line.Quantity);
                        if (requestedInStrnUnit > strnLine.BalanceToReceive)
                            throw new InvalidOperationException($"Attempt to exceed Balance Quantity for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, STRN balance remaining: {await _unitConversionService.ConvertUnitAsync(strnLine.Unit, line.Unit, strnLine.BalanceToReceive)} {line.Unit}.");

                        // 4. Lock and validate physical stock — hard block if issuing would exceed
                        // quantity actually in hand.
                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (stockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under store '{line.StoreCode}' does not exist in the stock master file.");

                        decimal requestedInStockUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.Quantity);
                        if (requestedInStockUnit > stockRecord.QtyInHand)
                            throw new InvalidOperationException($"Cannot issue below Quantity In Hand for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Qty In Hand: {await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, line.Unit, stockRecord.QtyInHand)} {line.Unit}.");

                        // 5. Soft check: issuing below the pre-calculated exact-consumption floor.
                        // Order-level aggregate across every style under this order/item (STRN has no
                        // style-level context to narrow this further) — takes the most conservative
                        // (highest) threshold among matches, per approved design: a false positive here
                        // only costs a manager one override click, a false negative would silently let
                        // a wasteful issue through.
                        var consumptionRows = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                            .AsNoTracking()
                            .Where(c => c.BuyerCode == header.BuyerCode && c.Order == header.Order && c.ItemCode == line.ItemCode)
                            .ToListAsync();

                        if (consumptionRows.Any())
                        {
                            decimal highestFloor = 0m;
                            foreach (var row in consumptionRows)
                            {
                                decimal totalConsumptionInLineUnit = await _unitConversionService.ConvertUnitAsync(row.ItemUnit, line.Unit, row.TotalConsumption);
                                decimal floor = row.PercentageAllowance == -100
                                    ? 0m
                                    : totalConsumptionInLineUnit / (100 + row.PercentageAllowance) * row.PercentageAllowance;
                                if (floor > highestFloor) highestFloor = floor;
                            }

                            decimal remainingAfterIssue = await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, line.Unit, stockRecord.QtyInHand) - line.Quantity;
                            if (remainingAfterIssue < highestFloor)
                            {
                                if (!isManagerOverrideAuthorized || !overrideExactConsumptionCheck)
                                {
                                    throw new ExactConsumptionOverrideRequiredException(
                                        $"Issuing this quantity of '{line.ItemCode}' would leave less than the pre-calculated exact-consumption floor ({highestFloor:0.00} {line.Unit}) remaining. Manager override required.");
                                }
                            }
                        }

                        // 6. Snapshot price/currency from the order-level master at issue time.
                        var masterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        // 7. Write the GIN transaction row.
                        var ginRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedGinNumber,
                            TransactionType = "4I",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                            DepartmentCode = header.DepartmentCode,
                            StockCode = line.StockCode,
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            SourceDocumentNumber = header.SourceStrnNumber,
                            Price = masterRow?.Price,
                            Currency = masterRow?.Currency,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(ginRow);

                        // 8. Update physical stock ledger.
                        stockRecord.QtyInHand -= requestedInStockUnit;
                        stockRecord.ToDateIssued += requestedInStockUnit;
                        stockRecord.StrnBalance -= requestedInStrnUnit;
                        stockRecord.LastDateIssued = header.TransactionDate;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);

                        // 9. Update order-level master running total.
                        if (masterRow != null)
                        {
                            decimal requestedInMasterUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, masterRow.Unit, line.Quantity);
                            masterRow.IssuedQuantity += requestedInMasterUnit;
                            _apparelProDbContext.OrderwiseStockMasters.Update(masterRow);
                        }

                        // 10. Decrement the STRN line's remaining balance.
                        strnLine.BalanceToReceive -= requestedInStrnUnit;
                        _apparelProDbContext.OrderwiseStockTransactions.Update(strnLine);
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

        public async Task<List<GinPendingStrnServiceModel>> GetPendingStrnsByOrderAsync(int buyerCode, string order)
        {
            order = order.Trim();

            // DepartmentCode/TransactionDate are denormalized identically across every line
            // of one STRN document (same assumption GetIssuableStrnLinesAsync already relies
            // on). Using Max() per group instead of First() avoids EF Core's occasionally-flaky
            // SQL translation of First() inside a grouped Select.
            var pendingStrns = await _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.BuyerCode == buyerCode && t.Order == order &&
                            t.TransactionType == "0S" && t.BalanceToReceive > 0)
                .GroupBy(t => t.DocumentNumber)
                .Select(g => new GinPendingStrnServiceModel
                {
                    StrnNumber = g.Key,
                    DepartmentCode = g.Max(x => x.DepartmentCode),
                    TransactionDate = g.Max(x => x.TransactionDate)
                })
                .OrderByDescending(s => s.TransactionDate)
                .ToListAsync();

            return pendingStrns;
        }
    }
}
