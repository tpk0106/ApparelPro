using apparelPro.BusinessLogic.Services.Models.OrderManagement.IScheduledShipmentsReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_RSHP1.PRG's "SCHEDULE SHIPMENT DETAIL REPORT" - see
    // ScheduledShipmentsReportServiceModel for why the legacy sha1/sha2/sha3 column
    // variants collapse into one shape here.
    public class ScheduledShipmentsReportService : IScheduledShipmentsReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public ScheduledShipmentsReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<ScheduledShipmentsReportServiceModel> GetScheduledShipmentsReportAsync(int? buyerCode, string? order)
        {
            order = order?.Trim();

            // Mirrors legacy's "Buyer Code Not Entered" error box - an Order without a Buyer is invalid.
            if (string.IsNullOrEmpty(order) == false && buyerCode == null)
                throw new InvalidOperationException("Buyer Code Not Entered.");

            var query = _apparelProDbContext.PartShipments.AsNoTracking();
            if (buyerCode.HasValue)
                query = query.Where(p => p.BuyerCode == buyerCode.Value);
            if (!string.IsNullOrEmpty(order))
                query = query.Where(p => p.Order == order);

            var shipments = await query
                .OrderBy(p => p.BuyerCode).ThenBy(p => p.Order).ThenBy(p => p.TypeCode)
                .ThenBy(p => p.StyleCode).ThenBy(p => p.ShipDate)
                .ToListAsync();

            // Mirrors legacy's "No Schedule Shipment Details For Printing" error box.
            if (shipments.Count == 0)
                throw new InvalidOperationException("No Schedule Shipment Details For Printing.");

            var buyerCodes = shipments.Select(p => p.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            // Type is always shown by name in this project's reports, never by raw code.
            var typeCodes = shipments.Select(p => p.TypeCode).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            var rows = shipments.Select(p => new ScheduledShipmentRowServiceModel
            {
                BuyerCode = p.BuyerCode,
                BuyerName = buyerNames.GetValueOrDefault(p.BuyerCode, p.BuyerCode.ToString()),
                Order = p.Order,
                TypeCode = p.TypeCode,
                TypeName = typeNames.GetValueOrDefault(p.TypeCode, ""),
                StyleCode = p.StyleCode,
                ShipmentOrderNo = p.NewOrder,
                Unit = p.Unit,
                Quantity = p.Quantity,
                DestinationCode = p.DestinationCode,
                ShipDate = p.ShipDate,
            }).ToList();

            return new ScheduledShipmentsReportServiceModel
            {
                BuyerCode = buyerCode,
                Order = order,
                Rows = rows,
            };
        }
    }
}
