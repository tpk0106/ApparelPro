using apparelPro.BusinessLogic.Services.interfaces.Reports.OrderManagement;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOutstandingPurchaseOrderListReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.OrderManagement
{
    // Replicates OD_PLST1.PRG's "LIST OF OUTSTANDING P/O's - Date Wise" report - see
    // OutstandingPurchaseOrderListReportServiceModel's notes for the corrected
    // "check every detail group" logic and the CreatedDate SCOPE NOTE.
    public class OutstandingPurchaseOrderListReportService : IOutstandingPurchaseOrderListReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public OutstandingPurchaseOrderListReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<OutstandingPurchaseOrderListReportServiceModel> GetOutstandingPurchaseOrderListReportAsync(
            DateOnly startDate, DateOnly endDate, string? basisCode)
        {
            var basisFilter = string.IsNullOrWhiteSpace(basisCode) ? null : basisCode.Trim();

            var headersQuery = _apparelProDbContext.SupplierPurchaseOrders
                .AsNoTracking()
                .Where(h => h.CreatedDate.HasValue && h.CreatedDate.Value >= startDate && h.CreatedDate.Value <= endDate);

            if (basisFilter != null)
                headersQuery = headersQuery.Where(h => h.StoreCode == basisFilter);

            var headers = await headersQuery.ToListAsync();

            if (headers.Count == 0)
                // Same wording as legacy's own error box for this exact condition.
                throw new InvalidOperationException("Given Date range not found.");

            var poNumbers = headers.Select(h => h.PurchaseOrderNumber).ToList();

            var details = await _apparelProDbContext.SupplierPurchaseOrderDetails
                .AsNoTracking()
                .Where(d => poNumbers.Contains(d.PONumber))
                .ToListAsync();

            // header.SupplierCode is always a stringified int - see
            // SupplierPurchaseOrderService.cs's write path (confirmed when building the
            // Purchase Order List Report). Parsing back here is safe, not a guess.
            var supplierCodesInt = headers
                .Select(h => int.TryParse(h.SupplierCode, out var sc) ? sc : (int?)null)
                .Where(sc => sc.HasValue)
                .Select(sc => sc!.Value)
                .Distinct()
                .ToList();
            var supplierNames = await _apparelProDbContext.Suppliers
                .AsNoTracking()
                .Where(s => supplierCodesInt.Contains(s.SupplierCode))
                .ToDictionaryAsync(s => s.SupplierCode, s => s.Name);

            var buyerCodes = details.Select(d => d.Buyer).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var typeCodes = details.Select(d => d.Type).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            var basisCodesNeeded = headers.Select(h => h.StoreCode).Distinct().ToList();
            var basisNames = await _apparelProDbContext.Basis
                .AsNoTracking()
                .Where(b => basisCodesNeeded.Contains(b.Code))
                .ToDictionaryAsync(b => b.Code, b => b.Description);

            var detailsByPo = details.GroupBy(d => d.PONumber).ToDictionary(g => g.Key, g => g.ToList());

            // For each qualifying P/O, find every Buyer/Order/Type/Style group that has
            // at least one line with a nonzero Balance - the corrected "outstanding"
            // test (checks ALL groups, not just the first one legacy happened to find).
            var outstandingByPo = new Dictionary<string, List<OutstandingPurchaseOrderGroupServiceModel>>();
            foreach (var header in headers)
            {
                if (!detailsByPo.TryGetValue(header.PurchaseOrderNumber, out var lines) || lines.Count == 0)
                    continue;

                var groups = lines
                    .GroupBy(l => new { l.Buyer, l.Order, l.Type, l.Style })
                    .Where(g => g.Any(l => l.Balance != 0))
                    .Select(g => new OutstandingPurchaseOrderGroupServiceModel
                    {
                        BuyerCode = g.Key.Buyer,
                        BuyerName = buyerNames.TryGetValue(g.Key.Buyer, out var bn) ? bn : "",
                        Order = g.Key.Order,
                        TypeCode = g.Key.Type,
                        TypeName = typeNames.TryGetValue(g.Key.Type, out var tn) ? tn : "",
                        StyleCode = g.Key.Style,
                    })
                    .OrderBy(g => g.Order, StringComparer.Ordinal)
                    .ThenBy(g => g.StyleCode, StringComparer.Ordinal)
                    .ToList();

                if (groups.Count > 0)
                    outstandingByPo[header.PurchaseOrderNumber] = groups;
            }

            var report = new OutstandingPurchaseOrderListReportServiceModel
            {
                StartDate = startDate,
                EndDate = endDate,
                BasisCode = basisFilter,
            };

            var basisGroupedHeaders = headers
                .Where(h => outstandingByPo.ContainsKey(h.PurchaseOrderNumber))
                .GroupBy(h => h.StoreCode)
                .OrderBy(g => g.Key, StringComparer.Ordinal);

            foreach (var basisGroup in basisGroupedHeaders)
            {
                var purchaseOrders = basisGroup
                    .OrderBy(h => h.CreatedDate)
                    .ThenBy(h => h.PurchaseOrderNumber, StringComparer.Ordinal)
                    .Select(h => new OutstandingPurchaseOrderServiceModel
                    {
                        PurchaseOrderNumber = h.PurchaseOrderNumber,
                        CreatedDate = h.CreatedDate,
                        CreatedTime = h.CreatedTime,
                        SupplierCode = h.SupplierCode,
                        SupplierName = int.TryParse(h.SupplierCode, out var scParsed)
                            && supplierNames.TryGetValue(scParsed, out var sName) ? sName : "",
                        ProformaInvoiceNo = h.ProformaInvoiceNo,
                        CurrencyCode = h.CurrencyCode,
                        OutstandingGroups = outstandingByPo[h.PurchaseOrderNumber],
                    })
                    .ToList();

                report.BasisGroups.Add(new OutstandingPurchaseOrderBasisGroupServiceModel
                {
                    BasisCode = basisGroup.Key,
                    BasisName = basisNames.TryGetValue(basisGroup.Key, out var basisName) ? basisName : "",
                    PurchaseOrders = purchaseOrders,
                });
            }

            return report;
        }
    }
}
