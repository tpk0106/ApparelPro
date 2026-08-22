using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService;
using apparelPro.BusinessLogic.SystemConfiguration;
using ApparelPro.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Dashboard
{
    // SRP: owns the home-dashboard read-models only (current-style
    // resolution, Production Progress aggregation). Deliberately separate
    // from DailyProductionEntryService/ProductionLineAllocationService,
    // whose job is transactional entry CRUD, not cross-table aggregation -
    // this class only reads.
    public class DashboardService : IDashboardService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISystemParameterLookupService _systemParameterLookupService;
        private readonly IUnitConversionService _unitConversionService;
        private readonly IStockMovementReportService _stockMovementReportService;

        public DashboardService(
            IMapper mapper, ApparelProDbContext apparelProDbContext,
            ISystemParameterLookupService systemParameterLookupService,
            IUnitConversionService unitConversionService,
            IStockMovementReportService stockMovementReportService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _systemParameterLookupService = systemParameterLookupService;
            _unitConversionService = unitConversionService;
            _stockMovementReportService = stockMovementReportService;
        }

        public async Task<CurrentStyleServiceModel?> GetCurrentStyleAsync()
        {
            // "Latest activity" wins over the pin whenever the floor has
            // logged anything at all - the pin only exists to give the
            // dashboard something to show before the first entry of a new
            // day/style has ever been typed in.
            var latestEntry = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .Select(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.Date })
                .FirstOrDefaultAsync();

            var latestTicket = await _apparelProDbContext.DailyProductionTimeTicketEntries
                .AsNoTracking()
                .OrderByDescending(t => t.Date)
                .Select(t => new { t.BuyerCode, t.Order, t.TypeCode, t.StyleCode, t.Date })
                .FirstOrDefaultAsync();

            var candidate = new[] { latestEntry, latestTicket }
                .Where(x => x != null)
                .OrderByDescending(x => x!.Date)
                .FirstOrDefault();

            if (candidate != null)
            {
                return new CurrentStyleServiceModel
                {
                    BuyerCode = candidate.BuyerCode,
                    Order = candidate.Order,
                    TypeCode = candidate.TypeCode,
                    StyleCode = candidate.StyleCode,
                    Source = "latest-entry"
                };
            }

            return await GetPinnedStyleAsync();
        }

        public async Task<ProductionProgressServiceModel> GetProductionProgressAsync(
            int buyerCode, string order, int typeCode, string styleCode)
        {
            var sections = await _apparelProDbContext.Sections
                .AsNoTracking()
                .OrderBy(s => s.Code)
                .ToListAsync();

            var toDateBySection = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.BuyerCode == buyerCode && e.Order == order &&
                            e.TypeCode == typeCode && e.StyleCode == styleCode)
                .GroupBy(e => e.SectionCode)
                .Select(g => new { SectionCode = g.Key, Total = g.Sum(e => e.Quantity) })
                .ToDictionaryAsync(g => g.SectionCode, g => g.Total);

            var contractSectionCode = await _systemParameterLookupService.GetValueAsync(
                SystemParameterKeys.ProductionContractSectionCode,
                SystemParameterKeys.ProductionContractSectionCodeDefault);
            var contractToDateQuantity = toDateBySection.GetValueOrDefault(contractSectionCode, 0);

            var lineAllocations = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .Where(a => a.BuyerCode == buyerCode && a.Order == order &&
                            a.TypeCode == typeCode && a.StyleCode == styleCode)
                .OrderBy(a => a.EstimatedStartDate)
                .ToListAsync();

            return new ProductionProgressServiceModel
            {
                ContractSectionCode = contractSectionCode,
                Sections = sections.Select(s => new SectionProgressServiceModel
                {
                    SectionCode = s.Code,
                    SectionDescription = s.Description,
                    ToDateQuantity = toDateBySection.GetValueOrDefault(s.Code, 0),
                    CeilingQuantity = contractToDateQuantity
                }).ToList(),
                LineAllocations = _mapper.Map<List<ProductionLineAllocationServiceModel>>(lineAllocations)
            };
        }

        public async Task<List<DailyTrendPointServiceModel>> GetDailyTrendAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            string? sectionCode, int days)
        {
            var resolvedSectionCode = sectionCode ?? await _systemParameterLookupService.GetValueAsync(
                SystemParameterKeys.ProductionContractSectionCode,
                SystemParameterKeys.ProductionContractSectionCodeDefault);

            // Summed across every line on that date - a factory-wide daily
            // output trend for the section, not scoped to a single line.
            var points = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.BuyerCode == buyerCode && e.Order == order &&
                            e.TypeCode == typeCode && e.StyleCode == styleCode &&
                            e.SectionCode == resolvedSectionCode)
                .GroupBy(e => e.Date)
                .Select(g => new DailyTrendPointServiceModel { Date = g.Key, Quantity = g.Sum(e => e.Quantity) })
                .OrderByDescending(p => p.Date)
                .Take(days)
                .ToListAsync();

            points.Reverse();
            return points;
        }

        public async Task<List<DailyTrendSeriesServiceModel>> GetDailyTrendAllSectionsAsync(
            int buyerCode, string order, int typeCode, string styleCode, int days)
        {
            var sections = await _apparelProDbContext.Sections
                .AsNoTracking()
                .OrderBy(s => s.Code)
                .ToListAsync();

            var raw = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.BuyerCode == buyerCode && e.Order == order &&
                            e.TypeCode == typeCode && e.StyleCode == styleCode)
                .GroupBy(e => new { e.Date, e.SectionCode })
                .Select(g => new { g.Key.Date, g.Key.SectionCode, Total = g.Sum(e => e.Quantity) })
                .ToListAsync();

            // The common date axis every section's series is aligned to -
            // the most recent `days` distinct dates with ANY entry, not per
            // section, so a section with no entry on a given date shows a
            // real 0 rather than the chart silently omitting that date.
            var recentDates = raw
                .Select(r => r.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .Take(days)
                .OrderBy(d => d)
                .ToList();

            return sections.Select(s => new DailyTrendSeriesServiceModel
            {
                SectionCode = s.Code,
                SectionDescription = s.Description,
                Points = recentDates.Select(d => new DailyTrendPointServiceModel
                {
                    Date = d,
                    Quantity = raw.FirstOrDefault(r => r.Date == d && r.SectionCode == s.Code)?.Total ?? 0
                }).ToList()
            }).ToList();
        }

        public async Task<OrderManagementSummaryServiceModel?> GetOrderManagementSummaryAsync(
            int buyerCode, string order, int typeCode, string styleCode)
        {
            var style = await _apparelProDbContext.Styles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order &&
                                           s.TypeCode == typeCode && s.StyleCode == styleCode);

            if (style == null) return null;

            // Shipments can be recorded in a different unit than the style's
            // own order unit (e.g. shipped in dozens while the order is
            // tracked in pieces) - each shipment's quantity is converted into
            // the style's unit before summing, rather than summed raw.
            var shipments = await _apparelProDbContext.PartShipments
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order == order &&
                            p.TypeCode == typeCode && p.StyleCode == styleCode)
                .Select(p => new { p.Unit, p.Quantity })
                .ToListAsync();

            decimal shippedQuantity = 0;
            if (!string.IsNullOrEmpty(style.Unit))
            {
                foreach (var shipment in shipments)
                {
                    shippedQuantity += await ConvertQuantityAsync(shipment.Unit, style.Unit, shipment.Quantity);
                }
            }
            else
            {
                shippedQuantity = shipments.Sum(s => s.Quantity);
            }

            var colorSizeMix = await _apparelProDbContext.ColorSizeDetails
                .AsNoTracking()
                .Where(c => c.BuyerCode == buyerCode && c.Order == order &&
                            c.TypeCode == typeCode && c.StyleCode == styleCode)
                .OrderByDescending(c => c.Qty)
                .Take(8)
                .Select(c => new ColorSizeMixServiceModel { Color = c.Color, Size = c.Size, Quantity = c.Qty })
                .ToListAsync();

            return new OrderManagementSummaryServiceModel
            {
                BuyerCode = style.BuyerCode,
                Order = style.Order,
                TypeCode = style.TypeCode,
                StyleCode = style.StyleCode,
                OrderQuantity = style.Quantity,
                Unit = style.Unit,
                UnitPrice = style.UnitPrice,
                OrderDate = style.OrderDate,
                EstimateApprovalDate = style.EstimateApprovalDate,
                ShippedQuantity = shippedQuantity,
                ColorSizeMix = colorSizeMix
            };
        }

        public async Task<OrderwiseInventorySummaryServiceModel?> GetOrderwiseInventorySummaryAsync(
            int buyerCode, string order)
        {
            try
            {
                var header = await _stockMovementReportService.GetStockMovementReportHeaderAsync(buyerCode, order);
                var lines = await _stockMovementReportService.GetStockMovementReportLinesForPdfAsync(buyerCode, order);

                return new OrderwiseInventorySummaryServiceModel
                {
                    TotalLineItems = header.TotalLineItems,
                    FullyReceivedCount = header.FullyReceivedCount,
                    DamagedItemCount = header.DamagedItemCount,
                    ShortfallCount = header.ShortfallCount,
                    // Top 8 by issued-vs-received ratio - the items closest to
                    // running out are the ones worth surfacing on a dashboard.
                    Items = lines
                        .OrderByDescending(l => l.ReceivedQuantity > 0 ? l.IssuedQuantity / l.ReceivedQuantity : 0)
                        .Take(8)
                        .Select(l => new StockItemMovementServiceModel
                        {
                            ItemCode = l.ItemCode,
                            Description = l.Description,
                            Unit = l.Unit,
                            ReceivedQuantity = l.ReceivedQuantity,
                            IssuedQuantity = l.IssuedQuantity,
                            BalanceQuantity = l.BalanceQuantity,
                            DamagedQuantity = l.DamagedQuantity,
                            IsLow = l.ReceivedQuantity > 0 && l.IssuedQuantity / l.ReceivedQuantity >= 0.85m
                        })
                        .ToList()
                };
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                // Same two "do error" cases GetStockMovementReportHeaderAsync
                // raises for the report screen (Buyer/Order not found, or no
                // items posted yet) - the dashboard just shows an empty state
                // instead of surfacing an exception.
                return null;
            }
        }

        private async Task<decimal> ConvertQuantityAsync(string fromUnit, string toUnit, decimal quantity)
        {
            if (string.Equals(fromUnit, toUnit, StringComparison.OrdinalIgnoreCase)) return quantity;
            var conversion = await _unitConversionService.GetUnitConversionByFromUnitAndToUnitAsync(fromUnit, toUnit);
            return conversion?.Measure is decimal measure ? quantity * measure : quantity;
        }

        private async Task<CurrentStyleServiceModel?> GetPinnedStyleAsync()
        {
            var buyerCodeText = await _systemParameterLookupService.GetValueAsync(
                SystemParameterKeys.DashboardPinnedBuyerCode, "");
            var order = await _systemParameterLookupService.GetValueAsync(
                SystemParameterKeys.DashboardPinnedOrder, "");
            var typeCodeText = await _systemParameterLookupService.GetValueAsync(
                SystemParameterKeys.DashboardPinnedTypeCode, "");
            var styleCode = await _systemParameterLookupService.GetValueAsync(
                SystemParameterKeys.DashboardPinnedStyleCode, "");

            if (string.IsNullOrEmpty(buyerCodeText) || string.IsNullOrEmpty(order) ||
                string.IsNullOrEmpty(typeCodeText) || string.IsNullOrEmpty(styleCode))
            {
                return null;
            }

            if (!int.TryParse(buyerCodeText, out var buyerCode) || !int.TryParse(typeCodeText, out var typeCode))
            {
                return null;
            }

            return new CurrentStyleServiceModel
            {
                BuyerCode = buyerCode,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                Source = "pinned"
            };
        }
    }
}
