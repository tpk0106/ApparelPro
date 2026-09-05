using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    public class GoodsReceivedNoteService : IGoodsReceivedNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GoodsReceivedNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<GrnPoLookupResultServiceModel> GetReceivableLinesByPoAsync(string purchaseOrderNumber)
        {
            purchaseOrderNumber = purchaseOrderNumber.Trim().ToUpper();

            // 1. Confirm the PO exists and grab its header context (Store/Supplier).
            var poHeaderRow = await _apparelProDbContext.SupplierPurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.PurchaseOrderNumber == purchaseOrderNumber);

            if (poHeaderRow == null)
                throw new KeyNotFoundException($"Purchase Order '{purchaseOrderNumber}' was not found.");

            // 2. Pull every PO line for this document in one query (no per-line round trips).
            var poLines = await _apparelProDbContext.SupplierPurchaseOrderDetails
                .AsNoTracking()
                .Where(d => d.PONumber == purchaseOrderNumber)
                .ToListAsync();

            var outstandingLines = poLines.Where(l => l.Balance > 0).ToList();
            if (!outstandingLines.Any())
                throw new InvalidOperationException($"Purchase Order '{purchaseOrderNumber}' has already been fully received.");

            // 3. Batch-load live stock context for every outstanding (buyer, order, item) —
            // same N+1 avoidance pattern already established in StoresRequisitionService/GoodsIssueService.
            // Two single-column Contains() (rather than a tuple Contains) keep this reliably
            // translatable to SQL; a PO normally spans one buyer/order anyway so the small
            // amount of over-fetching here is harmless.
            var buyerCodes = outstandingLines.Select(l => l.Buyer).Distinct().ToList();
            var orders = outstandingLines.Select(l => l.Order).Distinct().ToList();
            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.StoreCode == poHeaderRow.StoreCode &&
                            buyerCodes.Contains(s.BuyerCode) &&
                            orders.Contains(s.Order))
                .ToListAsync();
            var stockByBuyerOrderItem = stockRows.ToDictionary(s => (s.BuyerCode, s.Order, s.ItemCode));

            var resultLines = outstandingLines.Select(l =>
            {
                stockByBuyerOrderItem.TryGetValue((l.Buyer, l.Order, l.ItemCode), out var stock);
                return new GrnReceivableLineServiceModel
                {
                    Buyer = l.Buyer,
                    Order = l.Order,
                    Type = l.Type,
                    Style = l.Style,
                    ItemCode = l.ItemCode,
                    Unit = l.OrderUnit,
                    OrderQuantity = l.OrderQuantity,
                    Balance = l.Balance,
                    QtyInHand = stock?.QtyInHand ?? 0
                };
            }).ToList();

            return new GrnPoLookupResultServiceModel
            {
                PurchaseOrderNumber = purchaseOrderNumber,
                StoreCode = poHeaderRow.StoreCode,
                SupplierCode = poHeaderRow.SupplierCode,
                Lines = resultLines
            };
        }

        public async Task<bool> CommitGoodsReceivedNoteAsync(
            GrnHeaderServiceModel header,
            List<GrnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Goods Received Note cannot be committed without line items.");

            header.PurchaseOrderNumber = header.PurchaseOrderNumber.Trim().ToUpper();

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Re-derive header context from the PO itself — never trust a client echo.
                    var poHeaderRow = await _apparelProDbContext.SupplierPurchaseOrders
                        .AsNoTracking()
                        .FirstOrDefaultAsync(h => h.PurchaseOrderNumber == header.PurchaseOrderNumber);

                    if (poHeaderRow == null)
                        throw new InvalidOperationException($"Purchase Order '{header.PurchaseOrderNumber}' was not found.");

                    header.StoreCode = poHeaderRow.StoreCode;
                    header.SupplierCode = poHeaderRow.SupplierCode;

                    // 2. Thread-safe allocation of the next GRN number.
                    string allocatedGrnNumber = await _sharedService.GenerateNextDocumentNumberAsync("GRN");

                    foreach (var line in lines)
                    {
                        line.Order = line.Order.Trim();
                        line.Style = line.Style.Trim();
                        line.ItemCode = line.ItemCode.Trim();
                        line.Unit = line.Unit.Trim().ToUpper();

                        // 3. Lock and validate the matching PO detail line.
                        var poDetailRow = await _apparelProDbContext.SupplierPurchaseOrderDetails
                            .FromSqlInterpolated($@"SELECT * FROM SupplierPurchaseOrderDetails WITH (UPDLOCK, HOLDLOCK)
                                WHERE PONumber = {header.PurchaseOrderNumber}
                                  AND Buyer = {line.Buyer}
                                  AND [Order] = {line.Order}
                                  AND Type = {line.Type}
                                  AND Style = {line.Style}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (poDetailRow == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' was not found on Purchase Order '{header.PurchaseOrderNumber}'.");

                        decimal requestedInOrderUnit = await _sharedService.ConvertUnitAsync(line.Unit, poDetailRow.OrderUnit, line.Quantity);
                        if (requestedInOrderUnit > poDetailRow.Balance)
                            throw new InvalidOperationException($"Receipt Deficit: Attempted to receive more than the outstanding PO balance for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Outstanding: {await _sharedService.ConvertUnitAsync(poDetailRow.OrderUnit, line.Unit, poDetailRow.Balance)} {line.Unit}.");

                        // 4. Lock and validate the physical stock row (should already exist from PO placement).
                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {line.Buyer}
                                  AND [Order] = {line.Order}
                                  AND StoreCode = {header.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (stockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under store '{header.StoreCode}' does not exist in the stock master file.");

                        decimal requestedInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.Quantity);

                        // 5. Write the GRN transaction row. StockCode is decomposed from the leading
                        // 2 characters of the 22-char compound ItemCode — same convention already
                        // used in SupplierPurchaseOrderService.
                        var grnRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedGrnNumber,
                            TransactionType = "GR",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = line.Buyer,
                            Order = line.Order,
                            // GRN has no department context — the receiving Store doubles as the
                            // location/cost-center here, consistent with Department now serving as
                            // the unified store/department master (see Store field migration).
                            DepartmentCode = header.StoreCode,
                            StockCode = line.ItemCode.Substring(0, 2),
                            StoreCode = header.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            SourceDocumentNumber = header.PurchaseOrderNumber,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(grnRow);

                        // 6. Increment physical stock — the write nothing else in the system performs.
                        stockRecord.QtyInHand += requestedInStockUnit;
                        stockRecord.ToDateReceived += requestedInStockUnit;
                        stockRecord.LastDateReceived = header.TransactionDate;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);

                        // 7. Decrement the PO line's outstanding balance.
                        poDetailRow.Balance -= requestedInOrderUnit;
                        _apparelProDbContext.SupplierPurchaseOrderDetails.Update(poDetailRow);

                        // 8. Update order-level master running total.
                        var masterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {line.Buyer}
                                  AND [Order] = {line.Order}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (masterRow != null)
                        {
                            decimal requestedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, masterRow.Unit ?? "PCS", line.Quantity);
                            masterRow.ReceivedQuantity += requestedInMasterUnit;
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

        public async Task<List<GrnPendingPoServiceModel>> GetPendingPosByOrderAsync(int buyerCode, string order)
        {
            order = order.Trim();

            // 1. Find every PO number that still has an outstanding-balance line for this buyer+order.
            var outstandingPoNumbers = await _apparelProDbContext.SupplierPurchaseOrderDetails
                .AsNoTracking()
                .Where(d => d.Buyer == buyerCode && d.Order == order && d.Balance > 0)
                .Select(d => d.PONumber)
                .Distinct()
                .ToListAsync();

            if (!outstandingPoNumbers.Any())
                return new List<GrnPendingPoServiceModel>();

            // 2. Batch-load the matching headers in one query.
            var headerRows = await _apparelProDbContext.SupplierPurchaseOrders
                .AsNoTracking()
                .Where(h => outstandingPoNumbers.Contains(h.PurchaseOrderNumber))
                .ToListAsync();

            return headerRows.Select(h => new GrnPendingPoServiceModel
            {
                PurchaseOrderNumber = h.PurchaseOrderNumber,
                SupplierCode = h.SupplierCode,
                StoreCode = h.StoreCode
            }).ToList();
        }

        public async Task<GrnPrintDetailsServiceModel> GetGrnPrintDetailsAsync(string grnNumber)
        {
            grnNumber = grnNumber.Trim();

            var transactionRows = await _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == grnNumber && t.TransactionType == "GR")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (transactionRows.Count == 0)
                throw new KeyNotFoundException($"GRN No '{grnNumber}' not found.");

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

            // GoodsReceivedNoteService reuses SourceDocumentNumber to store the PO Number
            // (see CommitGoodsReceivedNoteAsync above) - Unit Price isn't snapshotted on the
            // "GR" transaction row itself, so it's resolved from SupplierPurchaseOrderDetails
            // instead, matched via PO Number + ItemCode - same convention as GrnListingReportService.
            string? poNumber = firstRow.SourceDocumentNumber?.Trim();
            var poDetails = !string.IsNullOrWhiteSpace(poNumber)
                ? await _apparelProDbContext.SupplierPurchaseOrderDetails.AsNoTracking().Where(d => d.PONumber == poNumber).ToListAsync()
                : new List<ApparelPro.Data.Models.OrderManagement.SupplierPurchaseOrderDetails>();
            var poDetailByItemCode = poDetails
                .GroupBy(d => d.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            var masters = await _apparelProDbContext.OrderwiseStockMasters
                .AsNoTracking()
                .Where(m => m.BuyerCode == firstRow.BuyerCode && m.Order.Trim() == firstRow.Order.Trim())
                .ToListAsync();
            var masterByItemCode = masters.GroupBy(m => m.ItemCode.Trim()).ToDictionary(g => g.Key, g => g.First());

            var lines = transactionRows.Select(t =>
            {
                var itemCode = t.ItemCode.Trim();
                var baseItemCode = DecomposePart(itemCode, 2, 4);

                profileByItemCode.TryGetValue(itemCode, out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(baseItemCode, out description);

                poDetailByItemCode.TryGetValue(itemCode, out var poDetail);
                masterByItemCode.TryGetValue(itemCode, out var master);

                decimal unitPrice = poDetail?.UnitPrice ?? 0;

                return new GrnPrintLineServiceModel
                {
                    ItemCode = itemCode,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    Unit = t.Unit,
                    Quantity = t.Quantity,
                    UnitPrice = unitPrice,
                    Value = t.Quantity * unitPrice,
                    Currency = master?.Currency ?? "",
                };
            }).ToList();

            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == firstRow.BuyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync();

            string? supplierName = null;
            if (firstRow.SupplierCode.HasValue)
            {
                supplierName = await _apparelProDbContext.Suppliers
                    .AsNoTracking()
                    .Where(s => s.SupplierCode == firstRow.SupplierCode.Value)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();
            }

            return new GrnPrintDetailsServiceModel
            {
                Header = new GrnPrintHeaderServiceModel
                {
                    GrnNumber = grnNumber,
                    BuyerCode = firstRow.BuyerCode,
                    BuyerName = !string.IsNullOrWhiteSpace(buyerName) ? buyerName : firstRow.BuyerCode.ToString(),
                    Order = firstRow.Order,
                    SupplierCode = firstRow.SupplierCode,
                    SupplierName = !string.IsNullOrWhiteSpace(supplierName) ? supplierName! : "",
                    PoNumber = poNumber ?? "",
                    StoreCode = firstRow.StoreCode,
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
