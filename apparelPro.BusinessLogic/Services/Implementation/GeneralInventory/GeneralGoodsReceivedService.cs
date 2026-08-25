using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Replicates GI_GRN1.PRG/GI_GRN2.PRG - General Inventory's Goods Received Note.
    //
    // Legacy fidelity notes (verified against the actual PRG source, not assumed):
    //   1. GI_GRN1.PRG never decrements gi_podet's Balance and never blocks receiving
    //      more than what's outstanding - unlike Orderwise's GRN. Replicated exactly:
    //      GeneralPurchaseOrderDetails.Balance is read for display context only, never
    //      written here.
    //   2. The only quantity gate is a soft Max Stock warning (no permission gate in
    //      legacy, just a plain Yes/No) - modeled as MaxStockOverrideRequiredException,
    //      distinct from GIN's manager-only MinStockOverrideRequiredException.
    //   3. Store is a per-line field (GeneralPurchaseOrderDetails.StoreCode), not
    //      header-level - GI's PO can span multiple stores, unlike Orderwise's PO.
    public class GeneralGoodsReceivedService : IGeneralGoodsReceivedService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;
        private readonly ICurrencyConversionService _currencyConversionService;

        public GeneralGoodsReceivedService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService,
            ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
            _currencyConversionService = currencyConversionService;
        }

        public async Task<GeneralGrnPoLookupResultServiceModel> GetReceivableLinesByPoAsync(string poNumber)
        {
            poNumber = poNumber.Trim();

            var poHeader = await _apparelProDbContext.GeneralPurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.PoNumber == poNumber);
            if (poHeader == null)
                throw new KeyNotFoundException("Invalid P/O No.");

            var poLines = await _apparelProDbContext.GeneralPurchaseOrderDetails
                .AsNoTracking()
                .Where(d => d.PoNumber == poNumber)
                .ToListAsync();
            if (poLines.Count == 0)
                throw new InvalidOperationException($"No items found for P/O No. '{poNumber}'.");

            var storeItemPairs = poLines.Select(l => (l.StoreCode, l.ItemCode)).Distinct().ToList();
            var storeCodes = storeItemPairs.Select(p => p.StoreCode).Distinct().ToList();
            var itemCodes = storeItemPairs.Select(p => p.ItemCode).Distinct().ToList();

            var masters = await _apparelProDbContext.GeneralStockMasters
                .AsNoTracking()
                .Where(m => storeCodes.Contains(m.StoreCode) && itemCodes.Contains(m.ItemCode))
                .ToListAsync();
            var masterByKey = masters.ToDictionary(m => (m.StoreCode, m.ItemCode));

            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            var lines = poLines.Select(l =>
            {
                masterByKey.TryGetValue((l.StoreCode, l.ItemCode), out var master);
                return new GeneralGrnReceivableLineServiceModel
                {
                    StoreCode = l.StoreCode,
                    ItemCode = l.ItemCode,
                    Description = descriptions.GetValueOrDefault(l.ItemCode, ""),
                    Unit = l.Unit,
                    OrderedQuantity = l.OrderedQuantity,
                    Balance = l.Balance,
                    QtyInHand = master?.QtyInHand ?? 0,
                    MaxStock = master?.MaxStock ?? 0,
                };
            }).ToList();

            return new GeneralGrnPoLookupResultServiceModel
            {
                PoNumber = poNumber,
                SupplierCode = poHeader.SupplierCode,
                CurrencyCode = poHeader.CurrencyCode,
                Lines = lines,
            };
        }

        public async Task<bool> CommitGeneralGoodsReceivedNoteAsync(
            GeneralGrnHeaderServiceModel header,
            List<GeneralGrnLineItemServiceModel> lines,
            string username,
            bool maxStockOverrideConfirmed)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Goods Received Note cannot be committed without line items.");

            header.PoNumber = header.PoNumber.Trim();
            header.CurrencyCode = header.CurrencyCode.Trim().ToUpper();
            header.SupplierCode = header.SupplierCode.Trim();

            var currency = await _apparelProDbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Code == header.CurrencyCode);
            if (currency == null)
                throw new InvalidOperationException("Invalid Currency Code.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                var poHeader = await _apparelProDbContext.GeneralPurchaseOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.PoNumber == header.PoNumber);
                if (poHeader == null)
                    throw new InvalidOperationException("Invalid P/O No.");

                var poLines = await _apparelProDbContext.GeneralPurchaseOrderDetails
                    .AsNoTracking()
                    .Where(d => d.PoNumber == header.PoNumber)
                    .ToListAsync();
                var poLineByKey = poLines.ToDictionary(d => (d.StoreCode, d.ItemCode));

                string allocatedGrnNumber = await _sharedService.GenerateNextDocumentNumberAsync("GGRN");

                foreach (var line in lines)
                {
                    line.StoreCode = line.StoreCode.Trim().ToUpper();
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();

                    if (!poLineByKey.ContainsKey((line.StoreCode, line.ItemCode)))
                        throw new InvalidOperationException($"Item '{line.ItemCode}' was not found on P/O No. '{header.PoNumber}'.");

                    var master = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {line.StoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();
                    if (master == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' not found in Item Master File.");

                    // Confirms a conversion rate exists between the header currency and the
                    // stock master's own currency - matches legacy's curconv(...)=0 guard.
                    decimal convertedPrice;
                    try
                    {
                        convertedPrice = await _currencyConversionService.ConvertAsync(line.Price, header.CurrencyCode, master.Currency);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException($"No conversion rate available from '{header.CurrencyCode}' to '{master.Currency}' for item '{line.ItemCode}'.");
                    }

                    decimal receiveQty = await _sharedService.ConvertUnitAsync(line.Unit, master.Unit, line.Quantity);

                    if (master.QtyInHand + receiveQty > master.MaxStock && !maxStockOverrideConfirmed)
                        throw new MaxStockOverrideRequiredException($"Entered Quantity Exceeds Maximum Stock for item '{line.ItemCode}'.");

                    master.QtyInHand += receiveQty;
                    master.Value += receiveQty * convertedPrice;
                    _apparelProDbContext.GeneralStockMasters.Update(master);

                    var grnRow = new GeneralStockTransaction
                    {
                        TransactionTypeCode = "0G",
                        DocumentNumber = allocatedGrnNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = line.StoreCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = line.Price,
                        Currency = header.CurrencyCode,
                        SupplierCode = header.SupplierCode,
                        InvoiceNumber = header.InvoiceNumber,
                        PoNumber = header.PoNumber,
                    };
                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(grnRow);
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

        public async Task<GeneralGrnPrintDetailsServiceModel> GetGeneralGrnPrintDetailsAsync(string grnNumber)
        {
            grnNumber = grnNumber.Trim();

            var rows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == grnNumber && t.TransactionTypeCode == "0G")
                .OrderBy(t => t.Id)
                .ToListAsync();
            if (rows.Count == 0)
                throw new KeyNotFoundException($"GRN No '{grnNumber}' not found.");

            var firstRow = rows[0];
            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new GeneralGrnPrintDetailsServiceModel
            {
                Header = new GeneralGrnPrintHeaderServiceModel
                {
                    GrnNumber = grnNumber,
                    PoNumber = firstRow.PoNumber ?? "",
                    SupplierCode = firstRow.SupplierCode ?? "",
                    CurrencyCode = firstRow.Currency ?? "",
                    InvoiceNumber = firstRow.InvoiceNumber,
                    TransactionDate = firstRow.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = rows.Select(r => new GeneralGrnPrintLineServiceModel
                {
                    StoreCode = r.StoreCode,
                    ItemCode = r.ItemCode,
                    Description = descriptions.GetValueOrDefault(r.ItemCode, ""),
                    Unit = r.Unit,
                    Quantity = r.Quantity,
                    Price = r.Price,
                }).ToList(),
            };
        }
    }
}
