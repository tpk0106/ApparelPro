using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_PLIST.PRG - "LIST OF P/O's (General)". A flat
    // GeneralPurchaseOrders listing for a date range - no transaction replay, this note
    // type never moves stock on its own. "Prepared By" reads UserId directly (this
    // codebase's own User.Identity.Name / email) rather than looking it up against
    // legacy's separate passs02 user-name table.
    public class GeneralPurchaseOrderListReportService : IGeneralPurchaseOrderListReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public GeneralPurchaseOrderListReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<(GeneralPurchaseOrderListReportHeaderServiceModel Header, List<GeneralPurchaseOrderListReportLineServiceModel> Lines)> BuildAsync(
            DateOnly fromDate, DateOnly toDate)
        {
            if (fromDate > toDate)
                throw new InvalidOperationException("Given Date Range not found.");

            var orders = await _apparelProDbContext.GeneralPurchaseOrders
                .AsNoTracking()
                .Where(o => o.OrderDate != null && o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .OrderBy(o => o.OrderDate)
                .ThenBy(o => o.OrderTime)
                .ThenBy(o => o.PoNumber)
                .ToListAsync();

            var supplierCodes = orders
                .Select(o => int.TryParse(o.SupplierCode, out var code) ? code : (int?)null)
                .Where(code => code.HasValue)
                .Select(code => code!.Value)
                .Distinct()
                .ToList();
            var supplierNames = await _apparelProDbContext.Suppliers
                .AsNoTracking()
                .Where(s => supplierCodes.Contains(s.SupplierCode))
                .ToDictionaryAsync(s => s.SupplierCode, s => s.Name);

            var lines = orders.Select(o => new GeneralPurchaseOrderListReportLineServiceModel
            {
                PoNumber = o.PoNumber,
                OrderDate = o.OrderDate,
                OrderTime = o.OrderTime,
                SupplierName = int.TryParse(o.SupplierCode, out var supplierCodeInt)
                    ? supplierNames.GetValueOrDefault(supplierCodeInt, o.SupplierCode)
                    : o.SupplierCode,
                BasisCode = o.BasisCode,
                ProformaInvoiceNo = o.ProformaInvoiceNo,
                CurrencyCode = o.CurrencyCode,
                PreparedBy = o.UserId ?? "",
            }).ToList();

            var header = new GeneralPurchaseOrderListReportHeaderServiceModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalLineItems = lines.Count,
            };

            return (header, lines);
        }

        public async Task<GeneralPurchaseOrderListReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate)
        {
            var (header, _) = await BuildAsync(fromDate, toDate);
            return header;
        }

        public async Task<List<GeneralPurchaseOrderListReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate)
        {
            var (_, lines) = await BuildAsync(fromDate, toDate);
            return lines;
        }
    }
}
