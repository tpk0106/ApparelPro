using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_DLIST.PRG - "LIST OF TRANSACTIONS". A flat
    // transaction log for a date range, optionally narrowed to one transaction type and/or
    // an item-code prefix (the "Stock/Item" filter is the first 6 characters of the 22-char
    // composite ItemCode - StockCode+ItemCode - not a physical Store filter; this report
    // spans every General Inventory store, matching legacy exactly). Unlike the Status/
    // Movement/Valuation/Re-order reports, this has no running balance and no totals.
    //
    // Same "5D" -> "6D" Damaged Goods Note deviation as GeneralStockMovementReportService,
    // for the same reason (every other source in this codebase uses "6D").
    public class GeneralTransactionListReportService : IGeneralTransactionListReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        private static readonly Dictionary<string, string> DocumentTypeDescriptions = new()
        {
            ["0G"] = "Goods Received Note",
            ["0S"] = "Stores Requisition Note",
            ["1TO"] = "Goods Transfer Note (Order)",
            ["1TG"] = "Goods Transfer Note (General)",
            ["2R"] = "Goods Return Note",
            ["3A"] = "Stock Adjustment Note",
            ["4I"] = "Goods Issue Note",
            ["6D"] = "Damaged Goods Note",
            ["6TO"] = "Goods Transfer Note (Order)",
            ["6TG"] = "Goods Transfer Note (General)",
            ["7SR"] = "Supplier Return Note (Regular)",
            ["7SD"] = "Supplier Return Note (Damaged)",
        };

        public GeneralTransactionListReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<(GeneralTransactionListReportHeaderServiceModel Header, List<GeneralTransactionListReportLineServiceModel> Lines)> BuildAsync(
            DateOnly fromDate, DateOnly toDate, string? transactionTypeCode, string? itemCodePrefix)
        {
            if (fromDate > toDate)
                throw new InvalidOperationException("[To Date] should be greater than [From Date].");

            transactionTypeCode = string.IsNullOrWhiteSpace(transactionTypeCode) ? null : transactionTypeCode.Trim().ToUpper();
            itemCodePrefix = string.IsNullOrWhiteSpace(itemCodePrefix) ? null : itemCodePrefix.Trim();

            var query = _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate);

            if (transactionTypeCode != null)
                query = query.Where(t => t.TransactionTypeCode == transactionTypeCode);

            if (itemCodePrefix != null)
                query = query.Where(t => t.ItemCode.StartsWith(itemCodePrefix));

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

            var lines = transactions.Select(t => new GeneralTransactionListReportLineServiceModel
            {
                TransactionDate = t.TransactionDate,
                TransactionTime = t.TransactionTime,
                TransactionTypeCode = t.TransactionTypeCode,
                DocumentTypeDescription = DocumentTypeDescriptions.GetValueOrDefault(t.TransactionTypeCode, t.TransactionTypeCode),
                DocumentNumber = t.DocumentNumber,
                StoreCode = t.StoreCode,
                ItemCode = t.ItemCode,
                Description = descriptions.GetValueOrDefault(t.ItemCode, ""),
                Quantity = t.Quantity,
                Unit = t.Unit,
            }).ToList();

            var header = new GeneralTransactionListReportHeaderServiceModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                TransactionTypeCode = transactionTypeCode,
                ItemCodePrefix = itemCodePrefix,
                TotalLineItems = lines.Count,
            };

            return (header, lines);
        }

        public async Task<GeneralTransactionListReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate, string? transactionTypeCode, string? itemCodePrefix)
        {
            var (header, _) = await BuildAsync(fromDate, toDate, transactionTypeCode, itemCodePrefix);
            return header;
        }

        public async Task<List<GeneralTransactionListReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate, string? transactionTypeCode, string? itemCodePrefix)
        {
            var (_, lines) = await BuildAsync(fromDate, toDate, transactionTypeCode, itemCodePrefix);
            return lines;
        }
    }
}
