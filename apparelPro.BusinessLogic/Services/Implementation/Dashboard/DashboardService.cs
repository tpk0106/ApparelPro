using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService;
using apparelPro.BusinessLogic.SystemConfiguration;
using ApparelPro.Data;
using ApparelPro.Data.Models.Dashboard;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
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
        // Threshold for the Order pipeline's "overdue" flag/banner - a style
        // sitting in the same stage longer than this is flagged. No system
        // parameter for this yet (matches this file's own precedent of a few
        // other hardcoded thresholds) - revisit as a configurable
        // SystemParameter if the business wants it tunable per-buyer/season.
        // TEMP (2026-09-06): dropped to -1 so the user can visually confirm
        // the overdue banner/chip/filter actually work, since the audit table
        // was just created and every style's DaysInStage is genuinely ~0
        // right now. -1 means "always overdue" - SET BACK TO 7 once confirmed.
        private const int OrderPipelineOverdueThresholdDays = -1;

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
                var (buyerName, typeName) = await ResolveBuyerAndTypeNamesAsync(candidate.BuyerCode, candidate.TypeCode);
                return new CurrentStyleServiceModel
                {
                    BuyerCode = candidate.BuyerCode,
                    BuyerName = buyerName,
                    Order = candidate.Order,
                    TypeCode = candidate.TypeCode,
                    TypeName = typeName,
                    StyleCode = candidate.StyleCode,
                    Source = "latest-entry"
                };
            }

            return await GetPinnedStyleAsync();
        }

        // FIXED: the dashboard's "current style" pill was printing raw
        // BuyerCode/TypeCode integers instead of their descriptions - same
        // bug, and same lookup-by-code fix, as GarmentAdditionalCostReport's
        // PDF header (see GarmentAdditionalCostService.GetGarmentAdditionalCostReportAsync).
        private async Task<(string buyerName, string typeName)> ResolveBuyerAndTypeNamesAsync(int buyerCode, int typeCode)
        {
            var buyerRow = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);

            var garmentTypeRow = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == typeCode);

            return (buyerRow?.Name ?? "", garmentTypeRow?.TypeName ?? "");
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

            var (buyerName, typeName) = await ResolveBuyerAndTypeNamesAsync(buyerCode, typeCode);

            return new CurrentStyleServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyerName,
                Order = order,
                TypeCode = typeCode,
                TypeName = typeName,
                StyleCode = styleCode,
                Source = "pinned"
            };
        }

        // Order pipeline (2026-09-06): one row per (BuyerCode, Order, TypeCode,
        // StyleCode), showing which of 6 lifecycle stages it's currently at -
        // Merchandising, Approval, Supplier PO, GRN, Production, Shipment,
        // or 6 (Complete/fully shipped). Every stage's "done" test is derived
        // from data that already exists elsewhere in the schema (see each
        // *StageDetailServiceModel's own comment for the specific fields and
        // why) - no new tables, no new columns. Deliberately does NOT include
        // "days in stage"/overdue flags from the original mockup - there is no
        // stored transition timestamp anywhere to compute that from.
        //
        // Bulk-loads each source table once and computes every style's stage
        // in memory, the same "load the whole table, join in C#" pattern
        // ItemWiseStockBalanceService and others already use for reports at
        // this scale - simpler and safer than hand-rolling 6+ separate
        // correlated-subquery joins in LINQ-to-SQL for a first version.
        public async Task<OrderPipelineResultServiceModel> GetOrderPipelineAsync(
            int? buyerCode, int? stage, string? search, bool? overdueOnly, int pageNumber, int pageSize)
        {
            var styles = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.Exported != true)
                .ToListAsync();

            if (styles.Count == 0)
            {
                return new OrderPipelineResultServiceModel();
            }

            var buyerCodesInScope = styles.Select(s => s.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodesInScope.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var breakdownCounts = (await _apparelProDbContext.ColorSizeDetails.AsNoTracking().ToListAsync())
                .GroupBy(c => (c.BuyerCode, Order: c.Order.Trim(), c.TypeCode, StyleCode: c.StyleCode.Trim()))
                .ToDictionary(g => g.Key, g => g.Count());

            var consumptionCounts = (await _apparelProDbContext.StyleMaterialConsumptionLedgers.AsNoTracking().ToListAsync())
                .GroupBy(c => (c.BuyerCode, Order: c.Order.Trim(), c.TypeCode, StyleCode: c.StyleCode.Trim()))
                .ToDictionary(g => g.Key, g => g.Count());

            var costProfilesByStyle = (await _apparelProDbContext.StyleMaterialCostProfiles.AsNoTracking().ToListAsync())
                .GroupBy(c => (c.BuyerCode, Order: c.Order.Trim(), c.TypeCode, StyleCode: c.StyleCode.Trim()))
                .ToDictionary(g => g.Key, g => g.ToList());

            var poDetailsByStyle = (await _apparelProDbContext.SupplierPurchaseOrderDetails.AsNoTracking().ToListAsync())
                .GroupBy(p => (BuyerCode: p.Buyer, Order: p.Order.Trim(), TypeCode: p.Type, StyleCode: p.Style.Trim()))
                .ToDictionary(g => g.Key, g => g.ToList());

            var stockMasterByBuyerOrderItem = (await _apparelProDbContext.OrderwiseStockMasters.AsNoTracking().ToListAsync())
                .GroupBy(m => (m.BuyerCode, Order: m.Order.Trim(), ItemCode: m.ItemCode.Trim()))
                .ToDictionary(g => g.Key, g => g.First());

            var productionSumByStyle = (await _apparelProDbContext.DailyProductionEntries.AsNoTracking().ToListAsync())
                .GroupBy(d => (d.BuyerCode, Order: d.Order.Trim(), d.TypeCode, StyleCode: d.StyleCode.Trim()))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            var lineAllocTargetByStyle = (await _apparelProDbContext.ProductionLineAllocations.AsNoTracking().ToListAsync())
                .GroupBy(l => (l.BuyerCode, Order: l.Order.Trim(), l.TypeCode, StyleCode: l.StyleCode.Trim()))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalQuantity));

            var shipmentSumByStyle = (await _apparelProDbContext.PartShipments.AsNoTracking().ToListAsync())
                .GroupBy(p => (p.BuyerCode, Order: p.Order.Trim(), p.TypeCode, StyleCode: p.StyleCode.Trim()))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            // Reconcile-on-read against OrderPipelineStageHistory: the first
            // time this endpoint ever sees a style at a given computed stage,
            // it stamps a row here - there is nowhere else in the schema this
            // "when did it get here" timestamp could come from. Never updates
            // an existing row (first-reached date is kept even if a style
            // somehow revisits a stage), and only ever writes NEW rows in one
            // batch after every style's stage is known, not per-style.
            var stageEnteredAtByStyle = (await _apparelProDbContext.OrderPipelineStageHistories.AsNoTracking().ToListAsync())
                .GroupBy(h => (h.BuyerCode, Order: h.Order.Trim(), h.TypeCode, StyleCode: h.StyleCode.Trim()))
                .ToDictionary(g => g.Key, g => g.ToDictionary(h => h.Stage, h => h.EnteredAt));

            var nowUtc = DateTime.UtcNow;
            var newHistoryRows = new List<OrderPipelineStageHistory>();

            var rows = new List<OrderPipelineRowServiceModel>();

            foreach (var style in styles)
            {
                (int BuyerCode, string Order, int TypeCode, string StyleCode) key =
                    (style.BuyerCode, style.Order.Trim(), style.TypeCode, style.StyleCode.Trim());

                var breakdownDone = breakdownCounts.GetValueOrDefault(key, 0) > 0;
                var consumptionDone = consumptionCounts.GetValueOrDefault(key, 0) > 0;
                var merchandising = new MerchandisingStageDetailServiceModel
                {
                    StyleSaved = true,
                    BreakdownDone = breakdownDone,
                    ConsumptionDone = consumptionDone,
                };

                var isApproved = style.ApprovedDate.HasValue;
                var approval = new ApprovalStageDetailServiceModel
                {
                    IsApproved = isApproved,
                    ApprovedBy = style.Username,
                    ApprovedDate = style.ApprovedDate,
                };

                var costProfiles = costProfilesByStyle.TryGetValue(key, out var cp) ? cp : new List<StyleMaterialCostProfile>();
                var outstandingLines = costProfiles.Where(c => c.BalanceQuantity > 0).ToList();
                var poDetails = poDetailsByStyle.TryGetValue(key, out var pd) ? pd : new List<SupplierPurchaseOrderDetails>();
                var supplierPo = new SupplierPoStageDetailServiceModel
                {
                    RaisedQuantity = poDetails.Sum(p => p.OrderQuantity),
                    RaisedValue = poDetails.Sum(p => p.OrderQuantity * p.UnitPrice),
                    OutstandingQuantity = outstandingLines.Sum(c => c.BalanceQuantity),
                    OutstandingValue = outstandingLines.Sum(c => c.BalanceQuantity * c.UnitPrice),
                    Currency = costProfiles.FirstOrDefault()?.Currency ?? "",
                };
                var poDone = costProfiles.Count == 0 || outstandingLines.Count == 0;

                decimal orderedQty = 0, receivedQty = 0, orderedValue = 0, receivedValue = 0;
                foreach (var poLine in poDetails)
                {
                    var stockKey = (style.BuyerCode, Order: key.Order, ItemCode: poLine.ItemCode.Trim());
                    if (stockMasterByBuyerOrderItem.TryGetValue(stockKey, out var master))
                    {
                        orderedQty += master.OrderedQuantity;
                        receivedQty += master.ReceivedQuantity;
                        orderedValue += master.OrderedQuantity * master.Price;
                        receivedValue += master.ReceivedQuantity * master.Price;
                    }
                }
                var grn = new GrnStageDetailServiceModel
                {
                    OrderedQuantity = orderedQty,
                    ReceivedQuantity = receivedQty,
                    OrderedValue = orderedValue,
                    ReceivedValue = receivedValue,
                };
                var grnDone = orderedQty <= 0 || receivedQty >= orderedQty;

                var actualProduction = productionSumByStyle.GetValueOrDefault(key, 0);
                var lineAllocTarget = lineAllocTargetByStyle.GetValueOrDefault(key, 0);
                var productionTarget = lineAllocTarget > 0 ? lineAllocTarget : (style.Quantity ?? 0);
                var production = new ProductionStageDetailServiceModel
                {
                    TargetQuantity = productionTarget,
                    ActualQuantity = actualProduction,
                };
                var productionDone = productionTarget <= 0 || actualProduction >= productionTarget;

                var scheduledShipment = shipmentSumByStyle.GetValueOrDefault(key, 0);
                var shipmentTarget = style.Quantity ?? 0;
                var unitPrice = style.UnitPrice ?? 0;
                var shipment = new ShipmentStageDetailServiceModel
                {
                    TargetQuantity = shipmentTarget,
                    ScheduledQuantity = scheduledShipment,
                    TargetValue = shipmentTarget * unitPrice,
                    ScheduledValue = scheduledShipment * unitPrice,
                };
                var shipmentDone = shipmentTarget <= 0 || scheduledShipment >= shipmentTarget;

                int computedStage;
                if (!(breakdownDone && consumptionDone)) computedStage = 0;
                else if (!isApproved) computedStage = 1;
                else if (!poDone) computedStage = 2;
                else if (!grnDone) computedStage = 3;
                else if (!productionDone) computedStage = 4;
                else if (!shipmentDone) computedStage = 5;
                else computedStage = 6;

                DateTime enteredAt;
                if (stageEnteredAtByStyle.TryGetValue(key, out var stageDates) &&
                    stageDates.TryGetValue(computedStage, out var existingEnteredAt))
                {
                    enteredAt = existingEnteredAt;
                }
                else
                {
                    enteredAt = nowUtc;
                    newHistoryRows.Add(new OrderPipelineStageHistory
                    {
                        BuyerCode = key.BuyerCode,
                        Order = key.Order,
                        TypeCode = key.TypeCode,
                        StyleCode = key.StyleCode,
                        Stage = computedStage,
                        EnteredAt = nowUtc,
                    });
                }
                var daysInStage = Math.Max(0, (int)(nowUtc - enteredAt).TotalDays);
                var isOverdue = computedStage < 6 && daysInStage > OrderPipelineOverdueThresholdDays;

                rows.Add(new OrderPipelineRowServiceModel
                {
                    BuyerCode = style.BuyerCode,
                    BuyerName = buyerNames.GetValueOrDefault(style.BuyerCode, style.BuyerCode.ToString()),
                    Order = key.Order,
                    TypeCode = style.TypeCode,
                    StyleCode = key.StyleCode,
                    Quantity = style.Quantity,
                    Unit = style.Unit,
                    Stage = computedStage,
                    DaysInStage = daysInStage,
                    IsOverdue = isOverdue,
                    Merchandising = merchandising,
                    Approval = approval,
                    SupplierPo = supplierPo,
                    Grn = grn,
                    Production = production,
                    Shipment = shipment,
                });
            }

            if (newHistoryRows.Count > 0)
            {
                _apparelProDbContext.OrderPipelineStageHistories.AddRange(newHistoryRows);
                await _apparelProDbContext.SaveChangesAsync();
            }

            var stageCounts = new int[7];
            foreach (var row in rows) stageCounts[row.Stage]++;
            var overdueCount = rows.Count(r => r.IsOverdue);

            IEnumerable<OrderPipelineRowServiceModel> filtered = rows;
            if (buyerCode.HasValue) filtered = filtered.Where(r => r.BuyerCode == buyerCode.Value);
            if (stage.HasValue) filtered = filtered.Where(r => r.Stage == stage.Value);
            if (overdueOnly == true) filtered = filtered.Where(r => r.IsOverdue);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToUpperInvariant();
                filtered = filtered.Where(r =>
                    r.BuyerName.ToUpperInvariant().Contains(term) ||
                    r.Order.ToUpperInvariant().Contains(term) ||
                    r.StyleCode.ToUpperInvariant().Contains(term));
            }

            var filteredList = filtered
                .OrderBy(r => r.Stage)
                .ThenBy(r => r.BuyerName)
                .ThenBy(r => r.Order)
                .ToList();

            var paged = filteredList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new OrderPipelineResultServiceModel
            {
                Items = paged,
                TotalItems = filteredList.Count,
                StageCounts = stageCounts,
                OverdueCount = overdueCount,
            };
        }
    }
}
