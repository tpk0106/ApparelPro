using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderDetailReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_RPO1.PRG's "ORDER CONFIRMATION REPORT" print program - see the SCOPE
    // NOTE on OrderDetailReportServiceModel for exactly which legacy case this covers
    // (Buyer+Order given, Type blank) and the two gaps found and deliberately not guessed
    // (order Description, resolved Destination names).
    public class OrderDetailReportService : IOrderDetailReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public OrderDetailReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<OrderDetailReportServiceModel> GetOrderDetailReportAsync(int buyerCode, string order)
        {
            order = order.Trim();

            // 1. Header - mirrors legacy's od_po seek (unit/curr), required before
            // OD_RPO1.PRG will print anything for this buyer+order.
            var purchaseOrder = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode && p.Order == order);

            if (purchaseOrder == null)
                throw new InvalidOperationException($"Buyer/Order not found in Order Confirmation: {buyerCode}/{order}.");

            // Same lookup-by-code pattern established for Trim Sheet Report's BuyerName.
            var buyerRow = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);
            var buyerName = buyerRow?.Name ?? "";

            // 2. Every Style under this Buyer+Order (od_style seek/skip-while loop),
            // ordered to match legacy's Type+Style key order.
            var styleRows = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .OrderBy(s => s.TypeCode)
                .ThenBy(s => s.StyleCode)
                .ToListAsync();

            var typeCodes = styleRows.Select(s => s.TypeCode).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            // 3. Every Part Shipment under this Buyer+Order in one query, then grouped by
            // Style in memory below - avoids one round-trip per style (od_part's own seek
            // in legacy is per-style since it's reading forward through a live cursor, but
            // there's no such constraint here).
            var partShipmentRows = await _apparelProDbContext.PartShipments
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order == order)
                .OrderBy(p => p.ShipDate)
                .ThenBy(p => p.NewOrder)
                .ToListAsync();

            var styles = new List<OrderDetailStyleServiceModel>();
            decimal grandTotal = 0;

            foreach (var styleRow in styleRows)
            {
                var unitPrice = styleRow.UnitPrice ?? 0;

                var partShipments = partShipmentRows
                    .Where(p => p.TypeCode == styleRow.TypeCode && p.StyleCode == styleRow.StyleCode)
                    .Select(p => new OrderDetailPartShipmentServiceModel
                    {
                        NewOrder = p.NewOrder ?? "",
                        DestinationCode = p.DestinationCode ?? "",
                        ShipDate = DateOnly.FromDateTime(p.ShipDate),
                        Unit = p.Unit ?? "",
                        Quantity = p.Quantity,
                        // Style's UnitPrice * this line's Quantity - matches legacy's
                        // "od_style->unit_pr*qty" (od_part.qty, not od_style.qty).
                        Value = unitPrice * p.Quantity,
                    })
                    .ToList();

                var styleTotalQuantity = partShipments.Sum(p => p.Quantity);
                var styleTotalValue = partShipments.Sum(p => p.Value);
                grandTotal += styleTotalValue;

                styles.Add(new OrderDetailStyleServiceModel
                {
                    TypeCode = styleRow.TypeCode,
                    TypeName = typeNames.TryGetValue(styleRow.TypeCode, out var typeName) ? typeName : "",
                    StyleCode = styleRow.StyleCode,
                    Unit = styleRow.Unit ?? "",
                    Quantity = styleRow.Quantity ?? 0,
                    UnitPrice = unitPrice,
                    PartShipments = partShipments,
                    TotalQuantity = styleTotalQuantity,
                    TotalValue = styleTotalValue,
                });
            }

            return new OrderDetailReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyerName,
                Order = order,
                OrderDate = DateOnly.FromDateTime(purchaseOrder.OrderDate),
                Unit = purchaseOrder.UnitCode ?? "",
                CurrencyCode = (purchaseOrder.CurrencyCode ?? "").Trim().ToUpperInvariant(),
                Styles = styles,
                GrandTotalValue = grandTotal,
            };
        }
    }
}
