using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_GRN3.PRG ("GRN LISTING - DATE WISE", optional
    // Basis filter) and IN_GRN4.PRG ("BUYER/ORDER GRN's LISTING", optional Supplier
    // filter). Rather than build two screens, this is one flexible report - leave
    // Date Range blank and supply Buyer/Order for the "Buyer/Order Wise" variant, or
    // supply a Date Range (Buyer/Order optional) for the "Date Wise" variant. At
    // least one of the two scopes is required.
    //
    // GoodsReceivedNoteService doesn't snapshot Price/Currency on its "GR" transaction
    // rows (unlike GIN/ARN) - Unit Price is resolved from SupplierPurchaseOrderDetails
    // instead, matched via SourceDocumentNumber (GRN reuses this column to store the
    // PO Number - see GoodsReceivedNoteService's own commit code) + ItemCode. That
    // same table's LCNo column covers legacy's L/C Number column too.
    public class GrnListingReportService : IGrnListingReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public GrnListingReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<(GrnListingReportHeaderServiceModel Header, List<GrnListingReportLineServiceModel> Lines)> BuildAsync(
            DateOnly? fromDate, DateOnly? toDate, int? buyerCode, string? order, string? storeCode, string? supplierCode)
        {
            bool hasDateRange = fromDate.HasValue && toDate.HasValue;
            bool hasBuyerOrder = buyerCode.HasValue && !string.IsNullOrWhiteSpace(order);
            if (!hasDateRange && !hasBuyerOrder)
                throw new InvalidOperationException("Enter a Date Range or a Buyer/Order to load GRN's.");
            if (hasDateRange && fromDate!.Value > toDate!.Value)
                throw new InvalidOperationException("Start date Cannot be greater than End date");

            order = string.IsNullOrWhiteSpace(order) ? null : order.Trim();
            storeCode = string.IsNullOrWhiteSpace(storeCode) ? null : storeCode.Trim().ToUpper();
            supplierCode = string.IsNullOrWhiteSpace(supplierCode) ? null : supplierCode.Trim();

            var query = _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.TransactionType == "GR");

            if (hasDateRange)
                query = query.Where(t => t.TransactionDate >= fromDate!.Value.ToDateTime(TimeOnly.MinValue) && t.TransactionDate <= toDate!.Value.ToDateTime(TimeOnly.MaxValue));
            if (hasBuyerOrder)
                query = query.Where(t => t.BuyerCode == buyerCode!.Value && t.Order == order);
            if (storeCode != null)
                query = query.Where(t => t.StoreCode == storeCode);

            string? supplierName = null;
            int? supplierCodeInt = null;
            if (supplierCode != null)
            {
                if (!int.TryParse(supplierCode, out var parsedSupplierCode))
                    throw new InvalidOperationException($"Invalid Supplier Code '{supplierCode}'.");
                var supplier = await _apparelProDbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierCode == parsedSupplierCode);
                if (supplier == null)
                    throw new InvalidOperationException($"Invalid Supplier Code '{supplierCode}'.");
                supplierCodeInt = parsedSupplierCode;
                supplierName = supplier.Name;
                query = query.Where(t => t.SupplierCode == parsedSupplierCode);
            }

            var transactions = await query
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.Id)
                .ToListAsync();

            if (transactions.Count == 0)
                throw new InvalidOperationException("No transactions to print.");

            var buyerOrderPairs = transactions.Select(t => (t.BuyerCode, Order: t.Order.Trim())).Distinct().ToList();
            var costProfiles = new List<ApparelPro.Data.Models.OrderManagement.MaterialConsumption.StyleMaterialCostProfile>();
            foreach (var (bc, ord) in buyerOrderPairs)
            {
                var rows = await _apparelProDbContext.StyleMaterialCostProfiles
                    .AsNoTracking()
                    .Where(p => p.BuyerCode == bc && p.Order.Trim() == ord)
                    .ToListAsync();
                costProfiles.AddRange(rows);
            }
            var profileByBuyerOrderItem = costProfiles
                .GroupBy(p => (p.BuyerCode, Order: p.Order.Trim(), ItemCode: p.ItemCode.Trim()))
                .ToDictionary(g => g.Key, g => g.First());

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = transactions.Select(t => DecomposePart(t.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var poNumbers = transactions.Where(t => !string.IsNullOrWhiteSpace(t.SourceDocumentNumber)).Select(t => t.SourceDocumentNumber!.Trim()).Distinct().ToList();
            var poDetails = poNumbers.Count > 0
                ? await _apparelProDbContext.SupplierPurchaseOrderDetails.AsNoTracking().Where(d => poNumbers.Contains(d.PONumber)).ToListAsync()
                : new List<ApparelPro.Data.Models.OrderManagement.SupplierPurchaseOrderDetails>();
            var poDetailByPoAndItem = poDetails
                .GroupBy(d => (d.PONumber.Trim(), ItemCode: d.ItemCode.Trim()))
                .ToDictionary(g => g.Key, g => g.First());

            var supplierCodesInLines = transactions.Where(t => t.SupplierCode.HasValue).Select(t => t.SupplierCode!.Value).Distinct().ToList();
            var supplierNames = supplierCodesInLines.Count > 0
                ? await _apparelProDbContext.Suppliers.AsNoTracking().Where(s => supplierCodesInLines.Contains(s.SupplierCode)).ToDictionaryAsync(s => s.SupplierCode, s => s.Name)
                : new Dictionary<int, string>();

            var buyerCodesInLines = transactions.Select(t => t.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodesInLines.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var masters = await _apparelProDbContext.OrderwiseStockMasters.AsNoTracking().ToListAsync();
            var masterByKey = masters
                .GroupBy(m => (m.BuyerCode, Order: m.Order.Trim(), ItemCode: m.ItemCode.Trim()))
                .ToDictionary(g => g.Key, g => g.First());

            var lines = transactions.Select(t =>
            {
                var itemCode = t.ItemCode.Trim();
                var ord = t.Order.Trim();
                var poNumber = t.SourceDocumentNumber?.Trim();

                profileByBuyerOrderItem.TryGetValue((t.BuyerCode, ord, itemCode), out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(DecomposePart(itemCode, 2, 4), out description);

                ApparelPro.Data.Models.OrderManagement.SupplierPurchaseOrderDetails? poDetail = null;
                if (poNumber != null)
                    poDetailByPoAndItem.TryGetValue((poNumber, itemCode), out poDetail);

                masterByKey.TryGetValue((t.BuyerCode, ord, itemCode), out var master);

                decimal unitPrice = poDetail?.UnitPrice ?? 0;
                string currency = master?.Currency ?? "";

                return new GrnListingReportLineServiceModel
                {
                    TransactionDate = DateOnly.FromDateTime(t.TransactionDate),
                    GrnNumber = t.DocumentNumber,
                    InvoiceNumber = t.InvoiceNumber,
                    PoNumber = poNumber,
                    LcNumber = poDetail?.LCNo,
                    StoreCode = t.StoreCode,
                    ItemCode = itemCode,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    Quantity = t.Quantity,
                    Unit = t.Unit,
                    UnitPrice = unitPrice,
                    Value = t.Quantity * unitPrice,
                    Currency = currency,
                    SupplierName = t.SupplierCode.HasValue ? supplierNames.GetValueOrDefault(t.SupplierCode.Value, "") : "",
                    BuyerCode = t.BuyerCode,
                    BuyerName = buyerNames.GetValueOrDefault(t.BuyerCode, ""),
                    Order = ord,
                };
            }).ToList();

            var header = new GrnListingReportHeaderServiceModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                BuyerCode = buyerCode,
                BuyerName = buyerCode.HasValue ? buyerNames.GetValueOrDefault(buyerCode.Value, "") : null,
                Order = order,
                StoreCode = storeCode,
                SupplierName = supplierName,
                TotalTransactions = lines.Count,
                TotalValue = lines.Sum(l => l.Value),
                TotalValueCurrency = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l.Currency))?.Currency,
            };

            return (header, lines);
        }

        public async Task<GrnListingReportHeaderServiceModel> GetHeaderAsync(
            DateOnly? fromDate, DateOnly? toDate, int? buyerCode, string? order, string? storeCode, string? supplierCode)
        {
            var (header, _) = await BuildAsync(fromDate, toDate, buyerCode, order, storeCode, supplierCode);
            return header;
        }

        public async Task<List<GrnListingReportLineServiceModel>> GetLinesAsync(
            DateOnly? fromDate, DateOnly? toDate, int? buyerCode, string? order, string? storeCode, string? supplierCode)
        {
            var (_, lines) = await BuildAsync(fromDate, toDate, buyerCode, order, storeCode, supplierCode);
            return lines;
        }
    }
}
