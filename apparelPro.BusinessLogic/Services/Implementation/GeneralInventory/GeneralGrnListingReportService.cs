using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_GRN4.PRG ("GRN LISTING - DATE WISE") and
    // GI_GRN5.PRG ("GRN LISTING - SUPPLIER WISE"). Both legacy screens list the exact
    // same underlying data (GeneralStockTransactions rows with TransactionTypeCode
    // "0G") for a date range - GI_GRN5 just adds Store/Supplier filters on top. Rather
    // than build two near-identical screens, this is one flexible report with
    // optional Store/Supplier filters, same pattern already used for the optional
    // item-code-prefix filter on GeneralTransactionListReportService.
    public class GeneralGrnListingReportService : IGeneralGrnListingReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public GeneralGrnListingReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<(GeneralGrnListingReportHeaderServiceModel Header, List<GeneralGrnListingReportLineServiceModel> Lines)> BuildAsync(
            DateOnly fromDate, DateOnly toDate, string? storeCode, string? supplierCode)
        {
            if (fromDate > toDate)
                throw new InvalidOperationException("Given date range not found.");

            storeCode = string.IsNullOrWhiteSpace(storeCode) ? null : storeCode.Trim().ToUpper();
            supplierCode = string.IsNullOrWhiteSpace(supplierCode) ? null : supplierCode.Trim();

            string? storeDescription = null;
            if (storeCode != null)
            {
                var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == storeCode);
                if (store == null)
                    throw new KeyNotFoundException($"Invalid Store Code '{storeCode}'.");
                storeDescription = store.Description;
            }

            string? supplierName = null;
            if (supplierCode != null)
            {
                if (int.TryParse(supplierCode, out var supplierCodeInt))
                {
                    var supplier = await _apparelProDbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierCode == supplierCodeInt);
                    if (supplier == null)
                        throw new KeyNotFoundException($"Invalid Supplier Code '{supplierCode}'.");
                    supplierName = supplier.Name;
                }
            }

            var query = _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.TransactionTypeCode == "0G" && t.TransactionDate >= fromDate && t.TransactionDate <= toDate);

            if (storeCode != null)
                query = query.Where(t => t.StoreCode == storeCode);
            if (supplierCode != null)
                query = query.Where(t => t.SupplierCode == supplierCode);

            var transactions = await query
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.TransactionTime)
                .ThenBy(t => t.Id)
                .ToListAsync();

            var itemCodes = transactions.Select(t => t.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            var storeCodes = transactions.Select(t => t.StoreCode).Distinct().ToList();
            var storeDescriptions = await _apparelProDbContext.GeneralStores
                .AsNoTracking()
                .Where(s => storeCodes.Contains(s.Code))
                .ToDictionaryAsync(s => s.Code, s => s.Description);

            var supplierCodesInt = transactions
                .Select(t => int.TryParse(t.SupplierCode, out var code) ? code : (int?)null)
                .Where(code => code.HasValue)
                .Select(code => code!.Value)
                .Distinct()
                .ToList();
            var supplierNames = await _apparelProDbContext.Suppliers
                .AsNoTracking()
                .Where(s => supplierCodesInt.Contains(s.SupplierCode))
                .ToDictionaryAsync(s => s.SupplierCode, s => s.Name);

            var lines = transactions.Select(t => new GeneralGrnListingReportLineServiceModel
            {
                TransactionDate = t.TransactionDate,
                GrnNumber = t.DocumentNumber,
                InvoiceNumber = t.InvoiceNumber,
                PoNumber = t.PoNumber,
                SupplierCode = t.SupplierCode,
                SupplierName = int.TryParse(t.SupplierCode, out var supCodeInt)
                    ? supplierNames.GetValueOrDefault(supCodeInt, t.SupplierCode ?? "")
                    : t.SupplierCode ?? "",
                StoreCode = t.StoreCode,
                StoreDescription = storeDescriptions.GetValueOrDefault(t.StoreCode, ""),
                ItemCode = t.ItemCode,
                Description = descriptions.GetValueOrDefault(t.ItemCode, ""),
                Unit = t.Unit,
                Quantity = t.Quantity,
                UnitPrice = t.Price,
                Amount = t.Quantity * t.Price,
            }).ToList();

            if (lines.Count == 0)
                throw new InvalidOperationException("No transactions to print.");

            var header = new GeneralGrnListingReportHeaderServiceModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                StoreCode = storeCode,
                StoreDescription = storeDescription,
                SupplierCode = supplierCode,
                SupplierName = supplierName,
                TotalTransactions = lines.Count,
                TotalValue = lines.Sum(l => l.Amount),
            };

            return (header, lines);
        }

        public async Task<GeneralGrnListingReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate, string? storeCode, string? supplierCode)
        {
            var (header, _) = await BuildAsync(fromDate, toDate, storeCode, supplierCode);
            return header;
        }

        public async Task<List<GeneralGrnListingReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate, string? storeCode, string? supplierCode)
        {
            var (_, lines) = await BuildAsync(fromDate, toDate, storeCode, supplierCode);
            return lines;
        }
    }
}
