using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPostOrderCostSheetReportService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_PCOST.PRG's "POST ORDER COST SHEET" report - a per-Buyer/Order
    // profitability sheet (materials + in-house production + additional costs + sub
    // contracts, weighed against sales value, finance charges and freight). Legacy
    // quirks deliberately NOT replicated (documented inline at each point):
    //   1. Interactive section picker: OD_PCOST.PRG lets the operator choose which
    //      non-final Sections to print via a dbedit multi-select before running the
    //      report - a dot-matrix print-length convenience, not a business rule (the
    //      final Section is always included regardless of the picker). This rebuild
    //      always returns every Section's quantity, same convention already used by
    //      Scheduled Shipments/Order Quota Detail's column selection.
    //   2. Sub Contract per-dozen bug: OD_PCOST.PRG recomputes mcos_p_doz on every
    //      loop iteration as (running total so far) + (this line's cost*12) instead of
    //      accumulating its own running total - this rebuild sums each line's own
    //      per-dozen cost once, same "sum once" fix already applied in
    //      CostOfProductionReportService's Additional Cost section.
    //   3. Production start date: OD_PCOST.PRG reads the first record off an index on
    //      sect_cd+date, so "go top" lands on the earliest date of whichever Section
    //      code sorts alphabetically first, not necessarily the true earliest
    //      production date. This rebuild takes the genuine MIN(Date) across every
    //      Section for the Buyer/Order.
    //   4. pr_mprod (stored running totals) doesn't exist in the modern schema by
    //      design (see DailyProductionEntry.cs) - section quantities are aggregated
    //      live from DailyProductionEntry instead, same convention as every other
    //      production report in this project.
    public class PostOrderCostSheetReportService : IPostOrderCostSheetReportService
    {
        private const string WorkHoursPerDayKey = "ProductionWorkHoursPerDay";

        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISystemParameterService _systemParameterService;
        private readonly ICurrencyConversionService _currencyConversionService;

        public PostOrderCostSheetReportService(
            ApparelProDbContext apparelProDbContext,
            ISystemParameterService systemParameterService,
            ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _systemParameterService = systemParameterService;
            _currencyConversionService = currencyConversionService;
        }

        public async Task<PostOrderCostSheetReportServiceModel> GetPostOrderCostSheetReportAsync(
            int buyerCode,
            string order,
            decimal percentOfTotalValue,
            decimal freightCharges,
            DateTime? actualShippedDate)
        {
            order = order?.Trim() ?? string.Empty;

            // Mirrors legacy's "Enter Buyer Code" / order-code required prompts.
            if (buyerCode <= 0 || string.IsNullOrEmpty(order))
                throw new InvalidOperationException("Buyer Code and Order Code are both required.");

            var purchaseOrder = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode && p.Order == order);
            if (purchaseOrder == null)
                throw new InvalidOperationException("Buyer/Order not found in Style File.");

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

            // ---- Header: styles, weighted average unit price ----

            var typeCodesForStyles = new HashSet<int>();
            var styles = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();
            foreach (var s in styles) typeCodesForStyles.Add(s.TypeCode);

            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodesForStyles.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            var styleRows = styles.Select(s => new PostOrderCostSheetStyleServiceModel
            {
                TypeCode = s.TypeCode,
                TypeName = typeNames.GetValueOrDefault(s.TypeCode, ""),
                StyleCode = s.StyleCode,
                Unit = s.Unit ?? "",
                UnitPrice = s.UnitPrice ?? 0,
                Quantity = s.Quantity ?? 0,
            }).ToList();

            var styleValueTotal = styleRows.Sum(s => s.UnitPrice * s.Quantity);
            var styleQtyTotal = purchaseOrder.TotalQuantity;
            var averageUnitPrice = styleQtyTotal == 0 ? 0 : styleValueTotal / styleQtyTotal;

            var partShipment = await _apparelProDbContext.PartShipments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode && p.Order == order);

            // ---- Section quantities (all sections, every Type/Style under this Buyer/Order) ----

            var sections = await _apparelProDbContext.Sections.AsNoTracking().ToListAsync();
            var finalSection = sections.FirstOrDefault(s => s.IsFinal);

            var quantityBySection = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order)
                .GroupBy(d => d.SectionCode)
                .Select(g => new { SectionCode = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(g => g.SectionCode, g => g.Quantity);

            var sectionQuantities = sections.Select(s => new PostOrderCostSheetSectionQuantityServiceModel
            {
                SectionCode = s.Code,
                SectionDescription = s.Description,
                IsFinal = s.IsFinal,
                Quantity = quantityBySection.GetValueOrDefault(s.Code, 0),
            }).ToList();

            var finalSectionQuantity = finalSection != null ? quantityBySection.GetValueOrDefault(finalSection.Code, 0) : 0;
            var totalValueOfSales = averageUnitPrice * finalSectionQuantity;

            var productionStartDate = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order)
                .OrderBy(d => d.Date)
                .Select(d => (DateOnly?)d.Date)
                .FirstOrDefaultAsync();

            var daysUtilised = finalSection == null
                ? 0
                : await _apparelProDbContext.DailyProductionEntries
                    .AsNoTracking()
                    .Where(d => d.BuyerCode == buyerCode && d.Order == order && d.SectionCode == finalSection.Code && d.Quantity != 0)
                    .Select(d => d.Date)
                    .Distinct()
                    .CountAsync();

            var averageDayProduction = daysUtilised == 0 ? 0 : Math.Round(finalSectionQuantity / daysUtilised, 0);

            // ---- Materials (value-add stores only, excluding items covered by the
            // Additional Cost section below), grouped by stock category ----

            var storeCodes = stockRows.Select(s => s.StoreCode).Distinct().ToList();
            var basisByCode = await _apparelProDbContext.Basis
                .AsNoTracking()
                .Where(b => storeCodes.Contains(b.Code))
                .ToDictionaryAsync(b => b.Code);

            var stockMasterByItem = await _apparelProDbContext.OrderwiseStockMasters
                .AsNoTracking()
                .Where(m => m.BuyerCode == buyerCode && m.Order == order)
                .ToDictionaryAsync(m => m.ItemCode);

            var stockCategoryDescByCode = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .ToDictionaryAsync(s => s.StockCode, s => s.Description);

            var additionalCostItems = await _apparelProDbContext.GarmentAdditionalCosts
                .AsNoTracking()
                .Where(a => a.BuyerCode == buyerCode && a.Order == order)
                .ToListAsync();
            var additionalCostItemCodes = additionalCostItems.Select(a => a.ItemCode).ToHashSet();

            var materialValueByCategory = new Dictionary<string, decimal>();
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
                var value = quantity * price;

                var categoryCode = stock.ItemCode.Length >= 2 ? stock.ItemCode.Substring(0, 2) : stock.ItemCode;
                materialValueByCategory[categoryCode] = materialValueByCategory.GetValueOrDefault(categoryCode, 0) + value;
            }

            var materialGroups = materialValueByCategory.Select(kv =>
            {
                var perPiece = finalSectionQuantity == 0 ? 0 : kv.Value / finalSectionQuantity;
                return new PostOrderCostSheetMaterialGroupServiceModel
                {
                    StockCategoryCode = kv.Key,
                    StockCategoryDescription = stockCategoryDescByCode.GetValueOrDefault(kv.Key, ""),
                    PerPieceCost = perPiece,
                    PerDozenCost = perPiece * 12,
                    TotalValue = kv.Value,
                };
            }).ToList();

            var materialsPerPieceCost = materialGroups.Sum(g => g.PerPieceCost);
            var materialsPerDozenCost = materialGroups.Sum(g => g.PerDozenCost);
            var materialsTotalValue = materialGroups.Sum(g => g.TotalValue);

            // ---- Production Cost (In House) - one summary figure across every line
            // allocated to this Buyer/Order ----

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

            var dailyEntriesForLines = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order && lineCodes.Contains(d.LineCode))
                .ToListAsync();

            var styleEndDateByKey = styles.ToDictionary(s => (s.TypeCode, s.StyleCode), s => s.ProductionEndDate);

            decimal productionCostPerPiece = 0;
            foreach (var allocation in lineAllocations)
            {
                productionLines.TryGetValue(allocation.LineCode, out var line);
                var costPerDay = line != null ? await ConvertAsync(allocation.CostPerDay, allocation.CurrencyCode) : 0;

                var endDate = styleEndDateByKey.GetValueOrDefault((allocation.TypeCode, allocation.StyleCode));
                var actualDays = dailyEntriesForLines
                    .Where(d => d.TypeCode == allocation.TypeCode && d.StyleCode == allocation.StyleCode && d.LineCode == allocation.LineCode)
                    .Where(d => endDate == null || endDate.Value >= d.Date)
                    .Sum(d => Math.Round(d.Hours / workHoursPerDay, 2));

                if (finalSectionQuantity != 0)
                    productionCostPerPiece += (costPerDay * actualDays) / finalSectionQuantity;
            }
            var productionCostPerDozen = productionCostPerPiece * 12;
            var productionCostTotalValue = productionCostPerPiece * finalSectionQuantity;

            // ---- Additional costs, grouped by Additional Cost code ----

            var additionalCostGroups = new List<PostOrderCostSheetAdditionalCostGroupServiceModel>();
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
                    decimal perPiece = 0;
                    decimal totalValue = 0;
                    foreach (var aitm in group)
                    {
                        if (!stockByItem.TryGetValue(aitm.ItemCode, out var stock))
                            continue; // legacy: "if found()" on in_stock - skip lines with no receiving record

                        decimal price = 0;
                        if (stockMasterByItem.TryGetValue(aitm.ItemCode, out var master))
                            price = await ConvertAsync(master.Price, GetMasterCurrency(master));

                        perPiece += price;
                        totalValue += stock.ToDateReceived * price;
                    }

                    additionalCostGroups.Add(new PostOrderCostSheetAdditionalCostGroupServiceModel
                    {
                        AdditionalCostCode = group.Key,
                        AdditionalCostDescription = additionalCostDescriptions.GetValueOrDefault(group.Key, ""),
                        PerPieceCost = perPiece,
                        PerDozenCost = perPiece * 12,
                        TotalValue = totalValue,
                    });
                }
            }
            var additionalCostPerPiece = additionalCostGroups.Sum(g => g.PerPieceCost);
            var additionalCostPerDozen = additionalCostGroups.Sum(g => g.PerDozenCost);
            var additionalCostTotalValue = additionalCostGroups.Sum(g => g.TotalValue);

            // ---- Sub contracts - one summary figure across every subcontractor ----

            var subContractRows = await _apparelProDbContext.SubContracts
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();

            decimal subContractPerPiece = 0;
            decimal subContractTotalValue = 0;
            foreach (var sc in subContractRows)
            {
                var costPerGarment = await ConvertAsync(sc.CostPerGarment, sc.Currency);
                subContractPerPiece += costPerGarment;
                subContractTotalValue += costPerGarment * sc.SubQuantity;
            }
            var subContractPerDozen = subContractPerPiece * 12;

            // ---- Totals ----

            var productionTotalPerPiece = productionCostPerPiece + additionalCostPerPiece + subContractPerPiece;
            var productionTotalPerDozen = productionCostPerDozen + additionalCostPerDozen + subContractPerDozen;
            var productionTotalValue = productionCostTotalValue + additionalCostTotalValue + subContractTotalValue;

            var grandTotalPerPiece = productionTotalPerPiece + materialsPerPieceCost;
            var grandTotalPerDozen = productionTotalPerDozen + materialsPerDozenCost;
            var grandTotalValue = productionTotalValue + materialsTotalValue;

            var grossProfit = totalValueOfSales - grandTotalValue;
            var financeCharges = (materialsTotalValue / 100m) * percentOfTotalValue;
            var netProfit = grossProfit - (financeCharges + freightCharges);
            var netProfitOnSalesPercent = totalValueOfSales == 0 ? 0 : (netProfit / totalValueOfSales) * 100;

            return new PostOrderCostSheetReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyer?.Name ?? buyerCode.ToString(),
                Order = order,
                CurrencyCode = orderCurrency,
                BasisCode = purchaseOrder.BasisCode ?? "",
                OrderDate = purchaseOrder.OrderDate,
                TotalOrderQuantity = purchaseOrder.TotalQuantity,
                Styles = styleRows,
                AverageUnitPrice = averageUnitPrice,
                PercentOfTotalValue = percentOfTotalValue,
                FreightCharges = freightCharges,
                ActualShippedDate = actualShippedDate,
                DeliveryOnDocumentDate = partShipment?.ShipDate,
                ProductionStartDate = productionStartDate?.ToDateTime(TimeOnly.MinValue),
                SectionQuantities = sectionQuantities,
                FinalSectionQuantity = finalSectionQuantity,
                TotalValueOfSales = totalValueOfSales,
                MaterialGroups = materialGroups,
                MaterialsPerPieceCost = materialsPerPieceCost,
                MaterialsPerDozenCost = materialsPerDozenCost,
                MaterialsTotalValue = materialsTotalValue,
                ProductionCostPerPiece = productionCostPerPiece,
                ProductionCostPerDozen = productionCostPerDozen,
                ProductionCostTotalValue = productionCostTotalValue,
                AdditionalCostGroups = additionalCostGroups,
                AdditionalCostPerPiece = additionalCostPerPiece,
                AdditionalCostPerDozen = additionalCostPerDozen,
                AdditionalCostTotalValue = additionalCostTotalValue,
                SubContractPerPiece = subContractPerPiece,
                SubContractPerDozen = subContractPerDozen,
                SubContractTotalValue = subContractTotalValue,
                ProductionTotalPerPiece = productionTotalPerPiece,
                ProductionTotalPerDozen = productionTotalPerDozen,
                ProductionTotalValue = productionTotalValue,
                GrandTotalPerPiece = grandTotalPerPiece,
                GrandTotalPerDozen = grandTotalPerDozen,
                GrandTotalValue = grandTotalValue,
                GrossProfit = grossProfit,
                FinanceCharges = financeCharges,
                DaysUtilised = daysUtilised,
                AverageDayProduction = averageDayProduction,
                NetProfit = netProfit,
                NetProfitOnSalesPercent = netProfitOnSalesPercent,
            };
        }

        private static string GetMasterCurrency(OrderwiseStockMaster master) => master.Currency;
    }
}
