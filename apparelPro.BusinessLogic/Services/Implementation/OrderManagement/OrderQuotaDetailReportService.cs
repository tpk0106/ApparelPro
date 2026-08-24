using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderQuotaDetailReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_ROQ1.PRG's "ORDER QUOTA REPORT" - see
    // OrderQuotaDetailReportServiceModel for why the legacy sass1/sass2/sass3 column
    // variants collapse into one shape here.
    public class OrderQuotaDetailReportService : IOrderQuotaDetailReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public OrderQuotaDetailReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<OrderQuotaDetailReportServiceModel> GetOrderQuotaDetailReportAsync(int? buyerCode, string? order)
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

            var quotas = await query
                .OrderBy(p => p.BuyerCode).ThenBy(p => p.Order).ThenBy(p => p.TypeCode)
                .ThenBy(p => p.StyleCode).ThenBy(p => p.NewOrder)
                .ToListAsync();

            // Mirrors legacy's "No Order Quota For Printing" error box.
            if (quotas.Count == 0)
                throw new InvalidOperationException("No Order Quota For Printing.");

            var buyerCodes = quotas.Select(p => p.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            // Type is always shown by name in this project's reports, never by raw code.
            var typeCodes = quotas.Select(p => p.TypeCode).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            var rows = quotas.Select(p => new OrderQuotaDetailRowServiceModel
            {
                BuyerCode = p.BuyerCode,
                BuyerName = buyerNames.GetValueOrDefault(p.BuyerCode, p.BuyerCode.ToString()),
                Order = p.Order,
                TypeCode = p.TypeCode,
                TypeName = typeNames.GetValueOrDefault(p.TypeCode, ""),
                StyleCode = p.StyleCode,
                ShipmentOrderNo = p.NewOrder,
                QuotaStatus = p.QuotaStatus,
                FromYearMonth = p.FromYearMonth,
                ToYearMonth = p.ToYearMonth,
                QuotaCategory = p.QuotaCategory,
                QuotaType = p.QuotaType,
                Unit = p.Unit,
                Quantity = p.Quantity,
            }).ToList();

            return new OrderQuotaDetailReportServiceModel
            {
                BuyerCode = buyerCode,
                Order = order,
                Rows = rows,
            };
        }
    }
}
