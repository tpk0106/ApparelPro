using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMonthlyActualShipmentsReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_ACTSP.PRG's "MONTHLY ACTUAL SHIPMENTS" report - every commercial
    // invoice line shipped within the given Month/Year (ie_coinv.ship_dt), with Value
    // computed as line quantity x the Style's unit price, same as legacy.
    public class MonthlyActualShipmentsReportService : IMonthlyActualShipmentsReportService
    {
        public MonthlyActualShipmentsReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private readonly ApparelProDbContext _apparelProDbContext;

        public async Task<MonthlyActualShipmentsReportServiceModel> GetMonthlyActualShipmentsReportAsync(int month, int year)
        {
            if (month < 1 || month > 12)
                throw new InvalidOperationException("Enter Month/Year in MM/YY Format.");

            var invoiceHeaders = await _apparelProDbContext.CommercialInvoiceHeaders
                .AsNoTracking()
                .Where(h => h.ShipDate != null && h.ShipDate.Value.Month == month && h.ShipDate.Value.Year == year)
                .ToListAsync();

            // Mirrors legacy's "No shipments details for printing" error box.
            if (invoiceHeaders.Count == 0)
                throw new InvalidOperationException("No shipments details for printing.");

            var shipDateByInvoice = invoiceHeaders.ToDictionary(h => h.InvoiceNumber, h => h.ShipDate!.Value);
            var invoiceNumbers = invoiceHeaders.Select(h => h.InvoiceNumber).ToList();

            var lines = await _apparelProDbContext.CommercialInvoiceLines
                .AsNoTracking()
                .Where(l => invoiceNumbers.Contains(l.InvoiceNumber))
                .OrderBy(l => l.InvoiceNumber).ThenBy(l => l.NewOrder)
                .ToListAsync();

            var buyerCodes = lines.Select(l => l.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var typeCodes = lines.Select(l => l.TypeCode).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            var styleUnitPrices = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => buyerCodes.Contains(s.BuyerCode))
                .ToDictionaryAsync(s => (s.BuyerCode, s.Order, s.TypeCode, s.StyleCode), s => s.UnitPrice ?? 0);

            var rows = lines.Select(l => new MonthlyActualShipmentRowServiceModel
            {
                InvoiceNumber = l.InvoiceNumber,
                BuyerCode = l.BuyerCode,
                BuyerName = buyerNames.GetValueOrDefault(l.BuyerCode, l.BuyerCode.ToString()),
                Order = l.Order,
                TypeCode = l.TypeCode,
                TypeName = typeNames.GetValueOrDefault(l.TypeCode, ""),
                OrderNo = l.NewOrder,
                StyleCode = l.StyleCode,
                ShipDate = shipDateByInvoice.GetValueOrDefault(l.InvoiceNumber),
                Quantity = l.Quantity,
                Balance = l.Balance,
                Value = l.Quantity * styleUnitPrices.GetValueOrDefault((l.BuyerCode, l.Order, l.TypeCode, l.StyleCode), 0),
            }).ToList();

            return new MonthlyActualShipmentsReportServiceModel
            {
                Month = month,
                Year = year,
                Rows = rows,
            };
        }
    }
}
