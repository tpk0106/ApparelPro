using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_ARN4.PRG (data entry/save, the version actually
    // wired up in IN_MENU.PRG - IN_ARN1 is superseded/unused) plus IN_ARN2.PRG (print).
    // "ADDITIONAL GOODS RECEIPTS NOTE" - the mirror image of Additional Issue Note
    // (AdditionalIssueNoteService): receives processed goods BACK from a Sub-Contractor
    // for a Buyer/Order's Additional Process (e.g. embroidery sent out, now returned),
    // increasing stock instead of decreasing it, and revaluing the order-level master
    // price using the note's entry currency.
    //
    // Structural deviation from AIN, confirmed by reading IN_ARN4.PRG in full: AIN has
    // one Buyer/Order/Process for the whole note (header-level); ARN lets every LINE
    // carry its own Buyer/Order/Process (temp->buyer/order/curr - "curr" is legacy's
    // reused field name for the Process code column here, confirmed by dtls[3]='curr'
    // paired with head[3]='Proc' in IN_ARN4.PRG's dbedit column definitions - nothing to
    // do with currency). A single ARN can therefore receive processed goods for several
    // different orders in one note.
    //
    // StoreCode is resolved server-side from the matched GarmentAdditionalCost row
    // (legacy: "m_store_cd = store_cd" once od_aitm is found) rather than client-supplied,
    // mirroring legacy exactly - the operator never picks a Store for an ARN line.
    //
    // Reuses OrderwiseStockTransaction.Price/Currency (previously documented as "GIN rows
    // only") and .SubContractorCode/.AdditionalProcessCode (already added for AIN) - Price/
    // Currency are generic "entered price/currency for this line" columns with no GIN-
    // specific business meaning, so widening their use here is not the same kind of field-
    // overloading this codebase otherwise avoids (e.g. AIN's SubContractorCode/
    // AdditionalProcessCode getting their own columns instead of reusing GTN's
    // CounterpartyBuyerCode/CounterpartyOrder, which really do carry different meanings).
    public class AdditionalGoodsReceiptNoteService : IAdditionalGoodsReceiptNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;
        private readonly ICurrencyConversionService _currencyConversionService;

        public AdditionalGoodsReceiptNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService,
            ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
            _currencyConversionService = currencyConversionService;
        }

        public async Task<List<ArnReceivableStockRowServiceModel>> GetReceivableStockByBuyerOrderAsync(
            int buyerCode, string order)
        {
            order = order.Trim();

            var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!buyerOrderExists)
                throw new KeyNotFoundException($"Invalid Buyer/Order (Buyer: {buyerCode}, Order: {order}).");

            var assignments = await _apparelProDbContext.GarmentAdditionalCosts
                .AsNoTracking()
                .Where(g => g.BuyerCode == buyerCode && g.Order == order)
                .ToListAsync();

            if (assignments.Count == 0)
                return new List<ArnReceivableStockRowServiceModel>();

            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();
            var stockByStoreItem = stockRows.ToDictionary(s => (s.StoreCode, s.ItemCode));

            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                .ToListAsync();
            var profileByItemCode = costProfiles
                .GroupBy(p => p.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            var resultList = new List<ArnReceivableStockRowServiceModel>();
            foreach (var assignment in assignments)
            {
                if (!stockByStoreItem.TryGetValue((assignment.StoreCode, assignment.ItemCode), out var stock))
                    continue;

                profileByItemCode.TryGetValue(assignment.ItemCode.Trim(), out var profile);

                resultList.Add(new ArnReceivableStockRowServiceModel
                {
                    ItemCode = assignment.ItemCode,
                    StoreCode = assignment.StoreCode,
                    Unit = assignment.Unit,
                    Description = profile?.Description ?? "",
                    AdditionalProcessCode = assignment.AdditionalCostCode,
                    ToDateIssued = stock.ToDateIssued,
                    ToDateReceived = stock.ToDateReceived,
                    IsSemiFinishedGarment = assignment.IsSemiFinishedGarment,
                    ReceivableBalance = stock.ToDateIssued - stock.ToDateReceived,
                });
            }

            return resultList.OrderBy(r => r.ItemCode).ToList();
        }

        public async Task<bool> CommitAdditionalGoodsReceiptNoteAsync(
            ArnHeaderServiceModel header,
            List<ArnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Additional Goods Receipt Note cannot be committed without line items.");

            header.SubContractorCode = header.SubContractorCode.Trim().ToUpper();
            header.Currency = header.Currency.Trim().ToUpper();

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Validate Sub-Contractor's Code - mirrors legacy "seek m_subcnt" on od_scref.
                    var subContractorExists = await _apparelProDbContext.SubContractors
                        .AsNoTracking()
                        .AnyAsync(s => s.Code == header.SubContractorCode);
                    if (!subContractorExists)
                        throw new InvalidOperationException($"Invalid Sub-Contractor Code '{header.SubContractorCode}'.");

                    // 2. Thread-safe allocation of the next ARN number.
                    string allocatedArnNumber = await _sharedService.GenerateNextDocumentNumberAsync("ARN");
                    header.ArnNumber = allocatedArnNumber;

                    foreach (var line in lines)
                    {
                        line.Order = line.Order.Trim();
                        line.AdditionalProcessCode = line.AdditionalProcessCode.Trim().ToUpper();
                        line.ItemCode = line.ItemCode.Trim();
                        line.Unit = line.Unit.Trim().ToUpper();

                        // Legacy: "valid m_qty > 0".
                        if (line.Quantity <= 0)
                            throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                        // 3. Re-validate the Buyer/Order - never trust a client echo.
                        var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                            .AsNoTracking()
                            .AnyAsync(po => po.BuyerCode == line.BuyerCode && po.Order == line.Order);
                        if (!buyerOrderExists)
                            throw new InvalidOperationException($"Invalid Buyer/Order (Buyer: {line.BuyerCode}, Order: {line.Order}).");

                        // 4. Validate Additional Process Code - mirrors legacy "seek m_adcost" on od_acost.
                        var additionalProcessExists = await _apparelProDbContext.AdditionalCosts
                            .AsNoTracking()
                            .AnyAsync(a => a.Code == line.AdditionalProcessCode);
                        if (!additionalProcessExists)
                            throw new InvalidOperationException($"Invalid Additional Process Code '{line.AdditionalProcessCode}'.");

                        // 5. Confirm this Buyer/Order/Item actually has that Additional Process
                        // assigned - mirrors legacy "seek m_buyer+m_order+m_adcost+m_item_cd" on
                        // od_aitm ("Item Code not found in Additional Cost File."). StoreCode is
                        // read off this row, never client-supplied (see class comment).
                        var assignment = await _apparelProDbContext.GarmentAdditionalCosts
                            .AsNoTracking()
                            .FirstOrDefaultAsync(g => g.BuyerCode == line.BuyerCode && g.Order == line.Order
                                && g.AdditionalCostCode == line.AdditionalProcessCode && g.ItemCode == line.ItemCode);
                        if (assignment == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' not found in Additional Cost File for Buyer/Order '{line.BuyerCode}/{line.Order}' and Process '{line.AdditionalProcessCode}'.");

                        // 6. Lock and validate the physical stock row - mirrors legacy
                        // "seek m_buyer+m_order+m_store_cd+m_item_cd" on in_stock.
                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {line.BuyerCode}
                                  AND [Order] = {line.Order}
                                  AND StoreCode = {assignment.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();
                        if (stockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under basis '{assignment.StoreCode}' does not exist in the stock master file.");

                        // 7. Unit compatibility + conversion - mirrors legacy's chk_unit()/convert().
                        decimal quantityInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.Quantity);

                        // 8. Receivable-balance cap - legacy only enforces this for semi-finished
                        // garments ("valid m_qty <= m_bal_rec" where m_bal_rec = to_dt_iss -
                        // to_dt_rec); non-semi-finished items have no cap beyond Quantity > 0.
                        if (assignment.IsSemiFinishedGarment)
                        {
                            decimal receivableBalance = stockRecord.ToDateIssued - stockRecord.ToDateReceived;
                            if (quantityInStockUnit > receivableBalance)
                                throw new InvalidOperationException($"Entered Quantity exceeds the outstanding Issued-but-not-Received balance for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Receivable: {await _sharedService.ConvertUnitAsync(stockRecord.Unit, line.Unit, receivableBalance)} {line.Unit}.");
                        }

                        // 9. Write the ARN transaction row. "0X" is the exact legacy code
                        // (IN_ARN4.PRG: "id with '0X'").
                        var arnRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedArnNumber,
                            TransactionType = "0X",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = line.BuyerCode,
                            Order = line.Order,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Length >= 2 ? line.ItemCode.Substring(0, 2) : line.ItemCode,
                            StoreCode = assignment.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            Price = line.Price,
                            Currency = header.Currency,
                            SubContractorCode = header.SubContractorCode,
                            AdditionalProcessCode = line.AdditionalProcessCode,
                            InvoiceNumber = header.InvoiceNumber,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(arnRow);

                        // 10. Physical receipt - mirrors legacy's "repl qty_in_hd with qty_in_hd+m_qty,
                        // to_dt_rec with to_dt_rec+m_qty, l_dt_rec with d_toc(xdate)".
                        stockRecord.QtyInHand += quantityInStockUnit;
                        stockRecord.ToDateReceived += quantityInStockUnit;
                        stockRecord.LastDateReceived = header.TransactionDate;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);

                        // 11. Update order-level master running total and revalue Price - mirrors
                        // legacy's "repl rcvd_qty with rcvd_qty+m_qty, bal_qty with bal_qty+m_qty,
                        // price with curconv(m_curr,curr,temp->price)" on in_stmst.
                        var masterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {line.BuyerCode}
                                  AND [Order] = {line.Order}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (masterRow != null)
                        {
                            decimal quantityInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, masterRow.Unit, line.Quantity);
                            masterRow.ReceivedQuantity += quantityInMasterUnit;
                            masterRow.Price = await _currencyConversionService.ConvertAsync(line.Price, header.Currency, masterRow.Currency);
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

        // Modern equivalent of legacy IN_ARN2.PRG - "seek m_docno+'0X'" then walks
        // in_sttr while docno+id matches, printing Item/Description/Unit/Qty/Price/
        // Value/Balance/Buyer/Order per line (every line can be a different Buyer/Order,
        // see class comment).
        public async Task<ArnPrintDetailsServiceModel> GetArnPrintDetailsAsync(string arnNumber)
        {
            arnNumber = arnNumber.Trim();

            var transactionRows = await _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == arnNumber && t.TransactionType == "0X")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (transactionRows.Count == 0)
                throw new KeyNotFoundException($"ARN No '{arnNumber}' not found.");

            var firstRow = transactionRows[0];

            // Description lookup, scoped per (Buyer, Order) pair since every line can
            // differ - same two-tier fallback (cost profile primary, StockItems catalog
            // fallback) already used by AIN/STRN print.
            var buyerOrderPairs = transactionRows.Select(t => (t.BuyerCode, Order: t.Order.Trim())).Distinct().ToList();
            var costProfiles = new List<ApparelPro.Data.Models.OrderManagement.MaterialConsumption.StyleMaterialCostProfile>();
            foreach (var (buyerCode, order) in buyerOrderPairs)
            {
                var rows = await _apparelProDbContext.StyleMaterialCostProfiles
                    .AsNoTracking()
                    .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                    .ToListAsync();
                costProfiles.AddRange(rows);
            }
            var profileByBuyerOrderItem = costProfiles
                .GroupBy(p => (p.BuyerCode, Order: p.Order.Trim(), ItemCode: p.ItemCode.Trim()))
                .ToDictionary(g => g.Key, g => g.First());

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = transactionRows.Select(t => DecomposePart(t.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            // Current OrderwiseStock balances (ToDateIssued - ToDateReceived) - see
            // ArnPrintLineServiceModel.BalanceToReceive's own comment for why this
            // replaces legacy's own inconsistent formula.
            var stockKeys = transactionRows.Select(t => (t.BuyerCode, Order: t.Order.Trim(), t.StoreCode, ItemCode: t.ItemCode.Trim())).Distinct().ToList();
            var stockByKey = new Dictionary<(int, string, string, string), ApparelPro.Data.Models.OrderwiseInventory.OrderwiseStock>();
            foreach (var key in stockKeys)
            {
                var stock = await _apparelProDbContext.OrderwiseStocks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.BuyerCode == key.BuyerCode && s.Order == key.Order && s.StoreCode == key.StoreCode && s.ItemCode == key.ItemCode);
                if (stock != null)
                    stockByKey[key] = stock;
            }

            var lines = transactionRows.Select(t =>
            {
                var itemCode = t.ItemCode.Trim();
                var order = t.Order.Trim();
                profileByBuyerOrderItem.TryGetValue((t.BuyerCode, order, itemCode), out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(DecomposePart(itemCode, 2, 4), out description);

                stockByKey.TryGetValue((t.BuyerCode, order, t.StoreCode, itemCode), out var stock);
                decimal balanceToReceive = stock != null ? stock.ToDateIssued - stock.ToDateReceived : 0;

                decimal price = t.Price ?? 0;

                return new ArnPrintLineServiceModel
                {
                    ItemCode = itemCode,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    Unit = t.Unit,
                    Quantity = t.Quantity,
                    UnitPrice = price,
                    Value = t.Quantity * price,
                    BalanceToReceive = balanceToReceive,
                    BuyerCode = t.BuyerCode,
                    Order = order,
                };
            }).ToList();

            return new ArnPrintDetailsServiceModel
            {
                Header = new ArnPrintHeaderServiceModel
                {
                    ArnNumber = arnNumber,
                    StoreCode = firstRow.StoreCode,
                    InvoiceNumber = firstRow.InvoiceNumber,
                    SubContractorCode = firstRow.SubContractorCode ?? "",
                    Currency = firstRow.Currency ?? "",
                    TransactionDate = firstRow.TransactionDate,
                    TotalValue = lines.Sum(l => l.Value),
                    PrintedOn = DateTime.Now,
                },
                Lines = lines,
            };
        }
    }
}
