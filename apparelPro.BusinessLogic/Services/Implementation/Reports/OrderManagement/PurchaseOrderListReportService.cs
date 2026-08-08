using apparelPro.BusinessLogic.Services.interfaces.Reports.OrderManagement;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderListReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.OrderManagement
{
    // Replicates OD_POLST.PRG's "PURCHASE ORDER LIST" print program - see
    // PurchaseOrderListReportServiceModel's SCOPE NOTE for the P/O Date/Time gap.
    public class PurchaseOrderListReportService : IPurchaseOrderListReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public PurchaseOrderListReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<List<string>> GetPurchaseOrderNumbersAsync()
        {
            return await _apparelProDbContext.PurchaseOrderHeaders
                .AsNoTracking()
                .OrderBy(h => h.PurchaseOrderNumber)
                .Select(h => h.PurchaseOrderNumber)
                .ToListAsync();
        }

        public async Task<PurchaseOrderListReportServiceModel> GetPurchaseOrderListReportAsync(string purchaseOrderNumber)
        {
            purchaseOrderNumber = purchaseOrderNumber.Trim();

            var header = await _apparelProDbContext.PurchaseOrderHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.PurchaseOrderNumber == purchaseOrderNumber);

            if (header == null)
                // Same wording as legacy's own error box for this exact condition.
                throw new InvalidOperationException("Purchase Order No. not found.");

            var detailRows = await _apparelProDbContext.PODetails
                .AsNoTracking()
                .Where(d => d.PONumber == purchaseOrderNumber)
                .ToListAsync();

            if (detailRows.Count == 0)
                throw new InvalidOperationException("No Details found for given P/O No.");

            // header.SupplierCode is always a stringified int - SupplierPurchaseOrderService
            // writes it via `header.SupplierCode.ToString()` from an int-typed
            // POHeaderServiceModel.SupplierCode, confirmed by reading that write path.
            // Parsing back here is safe, not a guess at the data's shape.
            string supplierName = "";
            if (int.TryParse(header.SupplierCode, out var supplierCodeInt))
            {
                var supplierRow = await _apparelProDbContext.Suppliers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SupplierCode == supplierCodeInt);
                supplierName = supplierRow?.Name ?? "";
            }

            var buyerCodes = detailRows.Select(d => d.Buyer).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var typeCodes = detailRows.Select(d => d.Type).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            // Description is resolved the same way legacy's od_sacc2 seek does: keyed on
            // the exact Buyer+Order+Type+Style+ItemCode this line was raised against.
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => buyerCodes.Contains(p.BuyerCode))
                .ToListAsync();
            var descriptionLookup = costProfiles.ToDictionary(
                p => (p.BuyerCode, p.Order, p.TypeCode, p.StyleCode, p.ItemCode),
                p => p.Description);

            var lines = detailRows
                .OrderBy(d => d.ItemCode, StringComparer.Ordinal)
                .Select(d => new PurchaseOrderListLineServiceModel
                {
                    ItemCode = d.ItemCode,
                    Description = descriptionLookup.TryGetValue(
                        (d.Buyer, d.Order, d.Type, d.Style, d.ItemCode), out var desc) ? desc : "",
                    OrderQuantity = d.OrderQuantity,
                    OrderUnit = d.OrderUnit,
                    UnitPrice = d.UnitPrice,
                    BuyerCode = d.Buyer,
                    BuyerName = buyerNames.TryGetValue(d.Buyer, out var bn) ? bn : "",
                    Order = d.Order,
                    TypeCode = d.Type,
                    TypeName = typeNames.TryGetValue(d.Type, out var tn) ? tn : "",
                    StyleCode = d.Style,
                })
                .ToList();

            return new PurchaseOrderListReportServiceModel
            {
                PurchaseOrderNumber = header.PurchaseOrderNumber,
                SupplierCode = header.SupplierCode,
                SupplierName = supplierName,
                ProformaInvoiceNo = header.ProformaInvoiceNo,
                ProformaInvoiceDate = header.ProformaInvoiceDate,
                CurrencyCode = header.CurrencyCode,
                Lines = lines,
            };
        }
    }
}
