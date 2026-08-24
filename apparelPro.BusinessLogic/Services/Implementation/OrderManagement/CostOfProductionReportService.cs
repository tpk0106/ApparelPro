using apparelPro.BusinessLogic.Services.Models.OrderManagement.ICostOfProductionReportService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_FCOST.PRG's "COST OF PRODUCTION" report - an Estimated vs Actual
    // profitability analysis. Legacy quirks deliberately NOT replicated (documented
    // inline at each point):
    //   1. Additional Cost running-total bug: OD_FCOST.PRG's m_acos_tot accumulates the
    //      *running* per-group subtotal on every line instead of each line's own cost,
    //      over-counting whenever a cost-type group has more than one item line. This
    //      rebuild sums each line's own cost once.
    //   2. pr_mprod (stored monthly/manpower running totals) doesn't exist in the modern
    //      schema by design (see DailyProductionEntry.cs) - actual produced quantity is
    //      aggregated live from DailyProductionEntry instead, same convention already
    //      used by every other production report in this project.
    public class CostOfProductionReportService : ICostOfProductionReportService
    {
        private const string WorkHoursPerDayKey = "ProductionWorkHoursPerDay";

        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISystemParameterService _systemParameterService;
        private readonly ICurrencyConversionService _currencyConversionService;

        public CostOfProductionReportService(
            ApparelProDbContext apparelProDbContext,
            ISystemParameterService systemParameterService,
            ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _systemParameterService = systemParameterService;
            _currencyConversionService = currencyConversionService;
        }

        public async Task<CostOfProductionReportServiceModel> GetCostOfProductionReportAsync(int buyerCode, string order)
        {
            order = order?.Trim() ?? string.Empty;

            if (buyerCode <= 0 || string.IsNullOrEmpty(order))
                throw new InvalidOperationException("Buyer Code and Order Code are both required.");

            var purchaseOrder = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode && p.Order == order);
            if (purchaseOrder == null)
                throw new InvalidOperationException("Buyer/Order not found in P/O Master File.");

            var orderCurrency = (purchaseOrder.CurrencyCode ?? "").Trim().ToUpperInvariant();

            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();
            if (stockRows.Count == 0)
                throw new InvalidOperationException("No Transactions entered for above Order.");

            var buyer = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);

            async Task<decimal> ConvertAsync(decimal amount, string fromCurrency) =>
                await _currencyConversionService.ConvertAsync(amount, fromCurrency, orderCurrency);

            // ---- Shared lookups used across multiple sections ----

            var storeCodes = stockRows.Select(s => s.StoreCode).Distinct().ToList();
            var basisByCode = await _apparelProDbContext.Basis
                .AsNoTracking()
                .Where(b => storeCodes.Contains(b.Code))
                .ToDictionaryAsync(b => b.Code);

            var stockMasterByItem = await _apparelProDbContext.OrderwiseStockMasters
                .AsNoTracking()
                .Where(m => m.BuyerCode == buyerCode && m.Order == order)
                .ToDictionaryAsync(m => m.ItemCode);

            // od_sacc2 lookup by Buyer+Order+ItemCode only (ignoring Type/Style) - matches
            // legacy's od_sac21 index used specifically for these item-only seeks.
            var materialCostProfilesByItem = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(m => m.BuyerCode == buyerCode && m.Order == order)
                .GroupBy(m => m.ItemCode)
                .ToDictionaryAsync(g => g.Key, g => g.First());

            var generalStockRefByItem = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .ToDictionaryAsync(g => g.ItemCode);

            var stockCategoryByCode = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .ToDictionaryAsync(s => s.StockCode, s => s.Description);

            string ResolveDescription(string itemCode) =>
                materialCostProfilesByItem.TryGetValue(itemCode, out var profile) ? profile.Description
                : generalStockRefByItem.TryGetValue(itemCode, out var general) ? general.Description
                : "";

            var additionalCostItems = await _apparelProDbContext.GarmentAdditionalCosts
                .AsNoTracking()
                .Where(a => a.BuyerCode == buyerCode && a.Order == order)
                .ToListAsync();
            var additionalCostItemCodes = additionalCostItems.Select(a => a.ItemCode).ToHashSet();

            // ---- 1. Materials received (value-add stores only, excluding items covered
            // by the Additional Cost section below) ----

            var materials = new List<CostOfProductionMaterialLineServiceModel>();
            foreach (var stock in stockRows)
            {
                if (!basisByCode.TryGetValue(stock.StoreCode, out var basis) || !basis.ValueAdd)
                    continue;
                if (additionalCostItemCodes.Contains(stock.ItemCode))
                    continue;

                stockMasterByItem.TryGetValue(stock.ItemCode, out var master);
                var price = master != null ? await ConvertAsync(master.Price, GetMasterCurrency(master)) : 0;

                var supplierReturn = master?.SupplierReturnQuantity ?? 0;
                var quantity = stock.ToDateReceived - supplierReturn;
                var stockCategoryCode = stock.ItemCode.Length >= 2 ? stock.ItemCode.Substring(0, 2) : stock.ItemCode;

                materials.Add(new CostOfProductionMaterialLineServiceModel
                {
                    ItemCode = stock.ItemCode,
                    StockCategoryCode = stockCategoryCode,
                    StockCategoryDescription = stockCategoryByCode.GetValueOrDefault(stockCategoryCode, ""),
                    Description = ResolveDescription(stock.ItemCode),
                    Unit = stock.Unit,
                    Quantity = quantity,
                    Price = price,
                    Value = quantity * price,
                });
            }
            var totalMaterialsValue = materials.Sum(m => m.Value);

            // ---- 2. Additional costs ----

            var additionalCostGroups = new List<CostOfProductionAdditionalCostGroupServiceModel>();
            if (additionalCostItems.Count > 0)
            {
                var additionalCostCodes = additionalCostItems.Select(a => a.AdditionalCostCode).Distinct().ToList();
                var additionalCostDescriptions = await _apparelProDbContext.AdditionalCosts
                    .AsNoTracking()
                    .Where(a => additionalCostCodes.Contains(a.Code))
                    .ToDictionaryAsync(a => a.Code, a => a.Description);

                var stockByItem = stockRows.ToDictionary(s => s.ItemCode);

                foreach (var group in additionalCostItems.GroupBy(a => a.AdditionalCostCode))
                {
                    var lines = new List<CostOfProductionAdditionalCostLineServiceModel>();
                    foreach (var aitm in group)
                    {
                        if (!stockByItem.TryGetValue(aitm.ItemCode, out var stock))
                            continue; // legacy: "if found()" on in_stock - skip lines with no receiving record

                        decimal price = 0;
                        if (stockMasterByItem.TryGetValue(aitm.ItemCode, out var master))
                            price = await _currencyConversionService.ConvertAsync(master.Price, GetMasterCurrency(master), orderCurrency);

                        var cost = aitm.Quantity == 0 ? 0 : price * (stock.ToDateReceived / aitm.Quantity);

                        lines.Add(new CostOfProductionAdditionalCostLineServiceModel
                        {
                            ItemCode = aitm.ItemCode,
                            Description = ResolveDescription(aitm.ItemCode),
                            QuantityPerGarment = aitm.Quantity,
                            ReceivedQuantity = stock.ToDateReceived,
                            Unit = aitm.Unit,
                            PricePerUnit = price,
                            Cost = cost,
                        });
                    }

                    additionalCostGroups.Add(new CostOfProductionAdditionalCostGroupServiceModel
                    {
                        AdditionalCostCode = group.Key,
                        AdditionalCostDescription = additionalCostDescriptions.GetValueOrDefault(group.Key, ""),
                        Lines = lines,
                        TotalCost = lines.Sum(l => l.Cost),
                    });
                }
            }
            var totalAdditionalCostValue = additionalCostGroups.Sum(g => g.TotalCost);

            // ---- 3. Sub contracts ----

            var subContractRows = await _apparelProDbContext.SubContracts
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();
            var subContractorCodes = subContractRows.Select(s => s.SubContractorCode).Distinct().ToList();
            var subContractorNames = await _apparelProDbContext.SubContractors
                .AsNoTracking()
                .Where(s => subContractorCodes.Contains(s.Code))
                .ToDictionaryAsync(s => s.Code, s => s.Name);

            var subContracts = new List<CostOfProductionSubContractLineServiceModel>();
            foreach (var sc in subContractRows)
            {
                var costPerGarment = await ConvertAsync(sc.CostPerGarment, sc.Currency);
                subContracts.Add(new CostOfProductionSubContractLineServiceModel
                {
                    SubContractorCode = sc.SubContractorCode,
                    SubContractorName = subContractorNames.GetValueOrDefault(sc.SubContractorCode, ""),
                    CostPerGarment = costPerGarment,
                    Quantity = sc.SubQuantity,
                    Unit = sc.Unit,
                    Cost = costPerGarment * sc.SubQuantity,
                });
            }
            var totalSubContractValue = subContracts.Sum(s => s.Cost);

            // ---- 4. Style revenue: Estimated (budgeted qty x price) vs Actual (produced
            // qty at the final production Section x price, plus sub-contract received qty) ----

            var styles = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();

            var finalSection = await _apparelProDbContext.Sections
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IsFinal);

            var producedQuantities = finalSection == null
                ? new Dictionary<(int, string), decimal>()
                : await _apparelProDbContext.DailyProductionEntries
                    .AsNoTracking()
                    .Where(d => d.BuyerCode == buyerCode && d.Order == order && d.SectionCode == finalSection.Code)
                    .GroupBy(d => new { d.TypeCode, d.StyleCode })
                    .Select(g => new { g.Key.TypeCode, g.Key.StyleCode, Quantity = g.Sum(x => x.Quantity) })
                    .ToDictionaryAsync(g => (g.TypeCode, g.StyleCode), g => g.Quantity);

            var subContractReceivedByStyle = subContractRows
                .GroupBy(s => (s.TypeCode, s.StyleCode))
                .ToDictionary(g => g.Key, g => g.Sum(s => s.ReceivedQuantity));

            var styleRevenues = styles.Select(style =>
            {
                var estimatedQty = style.Quantity ?? 0;
                var unitPrice = style.UnitPrice ?? 0;
                var actualQty = producedQuantities.GetValueOrDefault((style.TypeCode, style.StyleCode), 0);
                var subReceivedQty = subContractReceivedByStyle.GetValueOrDefault((style.TypeCode, style.StyleCode), 0);

                return new CostOfProductionStyleRevenueServiceModel
                {
                    TypeCode = style.TypeCode,
                    StyleCode = style.StyleCode,
                    UnitPrice = unitPrice,
                    Unit = style.Unit ?? "",
                    EstimatedQuantity = estimatedQty,
                    EstimatedValue = estimatedQty * unitPrice,
                    ActualProducedQuantity = actualQty,
                    ActualProducedValue = actualQty * unitPrice,
                    SubContractReceivedQuantity = subReceivedQty,
                    SubContractReceivedValue = subReceivedQty * unitPrice,
                };
            }).ToList();

            // ---- 5. Production line costing: Estimated (allocated days x cost) vs
            // Actual (worked hours / work-hours-per-day, cut off at each style's
            // production end date if set) ----

            var workHoursPerDay = await _systemParameterService.GetDecimalValueAsync(WorkHoursPerDayKey, 8m);

            var lineAllocations = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .Where(a => a.BuyerCode == buyerCode && a.Order == order)
                .ToListAsync();

            var lineCodes = lineAllocations.Select(a => a.LineCode).Distinct().ToList();
            var productionLines = await _apparelProDbContext.ProductionLines
                .AsNoTracking()
                .Where(l => lineCodes.Contains(l.LineCode))
                .ToDictionaryAsync(l => l.LineCode);

            var dailyEntries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order && lineCodes.Contains(d.LineCode))
                .ToListAsync();

            var styleEndDateByKey = styles.ToDictionary(s => (s.TypeCode, s.StyleCode), s => s.ProductionEndDate);

            var lineCosts = new List<CostOfProductionLineCostServiceModel>();
            foreach (var allocation in lineAllocations)
            {
                productionLines.TryGetValue(allocation.LineCode, out var line);
                var costPerDay = line != null
                    ? await ConvertAsync(allocation.CostPerDay, allocation.CurrencyCode)
                    : 0;

                var endDate = styleEndDateByKey.GetValueOrDefault((allocation.TypeCode, allocation.StyleCode));
                var actualDays = dailyEntries
                    .Where(d => d.TypeCode == allocation.TypeCode && d.StyleCode == allocation.StyleCode && d.LineCode == allocation.LineCode)
                    .Where(d => endDate == null || endDate.Value >= d.Date)
                    .Sum(d => Math.Round(d.Hours / workHoursPerDay, 1));

                lineCosts.Add(new CostOfProductionLineCostServiceModel
                {
                    TypeCode = allocation.TypeCode,
                    StyleCode = allocation.StyleCode,
                    LineCode = allocation.LineCode,
                    LineDescription = line?.Description ?? "",
                    CostPerDay = costPerDay,
                    EstimatedDays = allocation.NumberOfDays,
                    EstimatedCost = costPerDay * allocation.NumberOfDays,
                    ActualDays = actualDays,
                    ActualCost = costPerDay * actualDays,
                });
            }

            // ---- 6. Estimated material/additional-cost totals, from the budget (od_sacc3)
            // rather than what's actually been received (section 1/2 above) ----

            var consumptionLedgerRows = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                .AsNoTracking()
                .Where(l => l.BuyerCode == buyerCode && l.Order == order)
                .ToListAsync();

            decimal estimatedMaterialsValue = 0;
            decimal estimatedAdditionalCostValue = 0;
            foreach (var ledgerRow in consumptionLedgerRows)
            {
                var itemCode = ledgerRow.StockCode + ledgerRow.ItemCode + ledgerRow.Feature1 + ledgerRow.Feature2 + ledgerRow.Feature3 + ledgerRow.Feature4;
                if (!materialCostProfilesByItem.TryGetValue(itemCode, out var profile))
                    continue;

                var price = await ConvertAsync(profile.UnitPrice, profile.Currency);
                var value = ledgerRow.TotalConsumption * price;
                if (ledgerRow.IsAdditionalCost)
                    estimatedAdditionalCostValue += value;
                else
                    estimatedMaterialsValue += value;
            }

            // ---- 7. Final profit margins ----

            var estimatedRevenue = styleRevenues.Sum(s => s.EstimatedValue);
            var actualRevenue = styleRevenues.Sum(s => s.ActualProducedValue + s.SubContractReceivedValue);
            var estimatedLineCost = lineCosts.Sum(l => l.EstimatedCost);
            var actualLineCost = lineCosts.Sum(l => l.ActualCost);

            var estimatedProfitMargin = estimatedRevenue - (estimatedMaterialsValue + totalSubContractValue + estimatedAdditionalCostValue + estimatedLineCost);
            var actualProfitMargin = actualRevenue - (totalMaterialsValue + totalSubContractValue + totalAdditionalCostValue + actualLineCost);

            return new CostOfProductionReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyer?.Name ?? buyerCode.ToString(),
                Order = order,
                CurrencyCode = orderCurrency,
                TotalOrderQuantity = purchaseOrder.TotalQuantity,
                Unit = purchaseOrder.UnitCode ?? "",
                Materials = materials,
                TotalMaterialsValue = totalMaterialsValue,
                EstimatedMaterialsValue = estimatedMaterialsValue,
                AdditionalCostGroups = additionalCostGroups,
                TotalAdditionalCostValue = totalAdditionalCostValue,
                EstimatedAdditionalCostValue = estimatedAdditionalCostValue,
                SubContracts = subContracts,
                TotalSubContractValue = totalSubContractValue,
                StyleRevenues = styleRevenues,
                LineCosts = lineCosts,
                EstimatedProfitMargin = estimatedProfitMargin,
                ActualProfitMargin = actualProfitMargin,
            };
        }

        // OrderwiseStockMaster doesn't carry its own Currency in a directly-typed way
        // distinct from the report's working currency concept - it does store one
        // (in_stmst.curr), just isolated here for readability at each of the three call
        // sites above.
        private static string GetMasterCurrency(OrderwiseStockMaster master) => master.Currency;
    }
}
