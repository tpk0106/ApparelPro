using apparelPro.BusinessLogic.Services.Models.OrderManagement.IYearSeasonOrdersReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_RPO2.PRG's "ORDER CONFIRMATION REPORT" family (mr_dart1-4) - see
    // YearSeasonOrdersReportServiceModel for why the legacy 4 column variants collapse
    // into one shape here, same pattern as ScheduledShipmentsReportService.
    public class YearSeasonOrdersReportService : IYearSeasonOrdersReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public YearSeasonOrdersReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<YearSeasonOrdersReportServiceModel> GetYearSeasonOrdersReportAsync(int? year, string? season)
        {
            season = season?.Trim();

            var query = _apparelProDbContext.PurchaseOrders.AsNoTracking();
            if (year.HasValue)
                query = query.Where(p => p.OrderDate.Year == year.Value);
            if (!string.IsNullOrEmpty(season))
                query = query.Where(p => p.Season == season);

            var purchaseOrders = await query
                .OrderBy(p => p.BuyerCode).ThenBy(p => p.Order)
                .ToListAsync();

            // Mirrors legacy's "No Details For Printing" error box.
            if (purchaseOrders.Count == 0)
                throw new InvalidOperationException("No Details For Printing.");

            var buyerCodes = purchaseOrders.Select(p => p.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var seasonCodes = purchaseOrders.Select(p => p.Season).Distinct().ToList();
            var seasonDescriptions = await _apparelProDbContext.Seasons
                .AsNoTracking()
                .Where(s => seasonCodes.Contains(s.Code))
                .ToDictionaryAsync(s => s.Code, s => s.Description);

            // Single bulk fetch for every Style touching these orders, grouped in memory
            // below - avoids one round-trip per order (same pattern as
            // OrderDetailReportService's PartShipments fetch).
            var orderKeys = purchaseOrders.Select(p => (p.BuyerCode, p.Order)).ToList();
            var buyerCodesForStyles = orderKeys.Select(k => k.BuyerCode).Distinct().ToList();
            var orderCodesForStyles = orderKeys.Select(k => k.Order).Distinct().ToList();
            var styleRows = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => buyerCodesForStyles.Contains(s.BuyerCode) && orderCodesForStyles.Contains(s.Order))
                .OrderBy(s => s.TypeCode).ThenBy(s => s.StyleCode)
                .ToListAsync();

            var typeCodes = styleRows.Select(s => s.TypeCode).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            var rows = purchaseOrders.Select(po =>
            {
                var styles = styleRows
                    .Where(s => s.BuyerCode == po.BuyerCode && s.Order == po.Order)
                    .Select(s =>
                    {
                        var quantity = s.Quantity ?? 0;
                        var unitPrice = s.UnitPrice ?? 0;
                        return new YearSeasonOrderStyleServiceModel
                        {
                            TypeCode = s.TypeCode,
                            TypeName = typeNames.TryGetValue(s.TypeCode, out var typeName) ? typeName : "",
                            StyleCode = s.StyleCode,
                            Unit = s.Unit ?? "",
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            // Matches legacy's "xtot_cost = qty * unit_pr" - od_style's own
                            // qty/unit_pr, not part-shipment quantities (unlike Order Detail
                            // Report, which sums per-shipment value).
                            TotalValue = quantity * unitPrice,
                        };
                    })
                    .ToList();

                return new YearSeasonOrderRowServiceModel
                {
                    BuyerCode = po.BuyerCode,
                    BuyerName = buyerNames.GetValueOrDefault(po.BuyerCode, po.BuyerCode.ToString()),
                    Order = po.Order,
                    Description = po.Description,
                    CountryCode = po.CountryCode ?? "",
                    Unit = po.UnitCode ?? "",
                    TotalQuantity = po.TotalQuantity,
                    CurrencyCode = po.CurrencyCode ?? "",
                    SeasonCode = po.Season ?? "",
                    SeasonDescription = seasonDescriptions.GetValueOrDefault(po.Season ?? "", po.Season ?? ""),
                    OrderDate = DateOnly.FromDateTime(po.OrderDate),
                    Styles = styles,
                    GrandTotalValue = styles.Sum(s => s.TotalValue),
                };
            }).ToList();

            return new YearSeasonOrdersReportServiceModel
            {
                Year = year,
                Season = season,
                Rows = rows,
            };
        }
    }
}
