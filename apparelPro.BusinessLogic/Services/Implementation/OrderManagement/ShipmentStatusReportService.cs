using apparelPro.BusinessLogic.Services.Models.OrderManagement.IShipmentStatusReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_SHPST.PRG's "SHIPMENT STATUS REPORT" - cross-references the planned
    // shipment schedule (od_part / PartShipment) against what has actually been invoiced
    // and shipped (ie_coin2 / CommercialInvoiceLine, dated via ie_coinv / CommercialInvoiceHeader).
    public class ShipmentStatusReportService : IShipmentStatusReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public ShipmentStatusReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<ShipmentStatusReportServiceModel> GetShipmentStatusReportAsync(int buyerCode, string order)
        {
            order = order?.Trim() ?? string.Empty;

            // Mirrors legacy's mandatory Buyer/Order entry - OD_SHPST.PRG exits the screen
            // immediately if either is left empty, unlike Scheduled Shipments which allows
            // an unfiltered listing.
            if (buyerCode <= 0 || string.IsNullOrEmpty(order))
                throw new InvalidOperationException("Buyer Code and Order Code are both required.");

            var shipments = await _apparelProDbContext.PartShipments
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order == order)
                .OrderBy(p => p.TypeCode).ThenBy(p => p.StyleCode).ThenBy(p => p.ShipDate)
                .ToListAsync();

            // Mirrors the sibling Order Management reports' "no data" error box - the legacy
            // screen itself just prints an empty listing, but this project's other reports
            // (e.g. Scheduled Shipments) surface an explicit error instead for the frontend
            // to display inline, so this follows the same established convention.
            if (shipments.Count == 0)
                throw new InvalidOperationException("No Shipment Status Details For Printing.");

            // Single bulk fetch for every invoice line touching this Buyer+Order, instead of
            // legacy's per-row re-seek of ie_coin2 - equivalent result, avoids an N+1 query.
            var invoiceLines = await _apparelProDbContext.CommercialInvoiceLines
                .AsNoTracking()
                .Where(l => l.BuyerCode == buyerCode && l.Order == order)
                .ToListAsync();

            var invoiceNumbers = invoiceLines.Select(l => l.InvoiceNumber).Distinct().ToList();
            var invoiceDates = await _apparelProDbContext.CommercialInvoiceHeaders
                .AsNoTracking()
                .Where(h => invoiceNumbers.Contains(h.InvoiceNumber))
                .ToDictionaryAsync(h => h.InvoiceNumber, h => (DateTime?)h.InvoiceDate);

            var buyer = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);

            // Type is always shown by name in this project's reports, never by raw code -
            // same convention as Order Detail Report / Year/Season Wise Orders.
            var typeCodes = shipments.Select(s => s.TypeCode).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            var rows = shipments.Select(shipment =>
            {
                // Matches legacy's re-seek key: buyer+order+type+style+new_order.
                var matchingLines = invoiceLines
                    .Where(l => l.TypeCode == shipment.TypeCode
                        && l.StyleCode == shipment.StyleCode
                        && l.NewOrder == shipment.NewOrder)
                    .Select(l => new ShipmentStatusInvoiceLineServiceModel
                    {
                        QuantityShipped = l.Quantity,
                        InvoiceDate = invoiceDates.GetValueOrDefault(l.InvoiceNumber),
                        InvoiceNumber = l.InvoiceNumber,
                    })
                    .ToList();

                return new ShipmentStatusRowServiceModel
                {
                    TypeCode = shipment.TypeCode,
                    TypeName = typeNames.GetValueOrDefault(shipment.TypeCode, ""),
                    StyleCode = shipment.StyleCode,
                    ShipmentOrderNo = shipment.NewOrder,
                    Unit = shipment.Unit,
                    DestinationCode = shipment.DestinationCode,
                    InvoiceLines = matchingLines,
                    TotalQuantityShipped = matchingLines.Sum(l => l.QuantityShipped),
                    BalanceToShip = shipment.Balance,
                };
            }).ToList();

            return new ShipmentStatusReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyer?.Name ?? buyerCode.ToString(),
                Order = order,
                Rows = rows,
            };
        }
    }
}
