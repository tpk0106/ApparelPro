using apparelPro.BusinessLogic.Services.Models.OrderManagement.ITrimSheetReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_TRIM.PRG ("TRIM SHEET" print program) - see the SCOPE NOTE on
    // TrimSheetReportServiceModel for what legacy sections this does NOT yet cover
    // (Sub Contract costs, Production Line costs - neither exists anywhere in this
    // system yet, confirmed 2026-08-07).
    public class TrimSheetReportService : ITrimSheetReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IMaterialConsumptionService _materialConsumptionService;
        private readonly IStyleApprovalService _styleApprovalService;
        private readonly ICurrencyConversionService _currencyConversionService;

        public TrimSheetReportService(
            ApparelProDbContext apparelProDbContext,
            IMaterialConsumptionService materialConsumptionService,
            IStyleApprovalService styleApprovalService,
            ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _materialConsumptionService = materialConsumptionService;
            _styleApprovalService = styleApprovalService;
            _currencyConversionService = currencyConversionService;
        }

        public async Task<TrimSheetReportServiceModel> GetTrimSheetReportAsync(int buyerCode, string order, int typeCode, string styleCode, bool includeProfit)
        {
            order = order.Trim();
            styleCode = styleCode.Trim();

            // 1. Header - mirrors legacy's od_style seek (qty/unit/unit_pr) and od_po seek
            // (curr/basis), both required before OD_TRIM.PRG will even accept the entry screen.
            var styleRow = await _apparelProDbContext.Styles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order && s.TypeCode == typeCode && s.StyleCode == styleCode);

            if (styleRow == null)
                throw new InvalidOperationException($"Buyer/Order/Type/Style not found in Style File: {buyerCode}/{order}/{typeCode}/{styleCode}.");

            var purchaseOrder = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode && p.Order == order);

            if (purchaseOrder == null)
                throw new InvalidOperationException($"Buyer/Order not found in Order Confirmation: {buyerCode}/{order}.");

            var orderCurrency = (purchaseOrder.CurrencyCode ?? "").Trim().ToUpperInvariant();
            var styleQuantity = styleRow.Quantity ?? 0;
            var unitPrice = styleRow.UnitPrice ?? 0;

            var basisDescription = "";
            if (!string.IsNullOrWhiteSpace(purchaseOrder.BasisCode))
            {
                var basisRow = await _apparelProDbContext.Basis
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Code == purchaseOrder.BasisCode.Trim());
                basisDescription = basisRow?.Description ?? "";
            }

            // FIXED (2026-08-07): Buyer Name and Garment Type description were never resolved
            // for this report - the header only ever had the raw BuyerCode/TypeCode to print.
            // Same lookup-by-code pattern as BasisDescription above.
            var buyerRow = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);
            var buyerName = buyerRow?.Name ?? "";

            var garmentTypeRow = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == typeCode);
            var typeName = garmentTypeRow?.TypeName ?? "";

            // 2. Material lines - reuse MaterialConsumptionService's own join logic
            // (descriptions, supplier names, cost-profile price/currency) rather than
            // re-implementing the same StockCode+ItemCode+Feature1-4 composite-key lookup here.
            var ledgerRows = await _materialConsumptionService.GetLedgerEntriesByStyleAsync(buyerCode, order, typeCode, styleCode);

            var stockCodes = ledgerRows.Select(l => l.StockCode).Distinct().ToList();
            var stockDescriptions = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .Where(s => stockCodes.Contains(s.StockCode))
                .ToDictionaryAsync(s => s.StockCode, s => s.Description);

            var lines = new List<TrimSheetLineServiceModel>();
            foreach (var row in ledgerRows)
            {
                // A line whose cost profile never recorded a currency (Currency comes back "")
                // is treated as already being in the order's own currency - a safe no-op
                // conversion - rather than raising the "missing rate" error, which is reserved
                // for a genuinely mismatched, known currency pair with no rate on file.
                var lineCurrency = string.IsNullOrWhiteSpace(row.Currency) ? orderCurrency : row.Currency.Trim().ToUpperInvariant();
                var convertedPrice = await _currencyConversionService.ConvertAsync(row.UnitPrice, lineCurrency, orderCurrency);
                var value = row.TotalConsumption * convertedPrice;

                lines.Add(new TrimSheetLineServiceModel
                {
                    StockCode = row.StockCode,
                    StockDescription = stockDescriptions.TryGetValue(row.StockCode, out var stockDesc) ? stockDesc : "",
                    ItemCode = row.ItemCode,
                    Description = row.Description,
                    Feature1 = row.Feature1,
                    Feature2 = row.Feature2,
                    Feature3 = row.Feature3,
                    Feature4 = row.Feature4,
                    IsConsumptionCalculated = row.CalculateConsumption,
                    QuantityPerGarment = row.QuantityPerGarment,
                    ConsumptionUnit = row.ConsumptionUnit,
                    TotalConsumption = row.TotalConsumption,
                    ItemUnit = row.ItemUnit,
                    ConvertedUnitPrice = convertedPrice,
                    Value = value,
                    SupplierCode = row.SupplierCode,
                    SupplierName = row.SupplierName,
                });
            }

            // 3. Per-stock-code subtotal blocks - GroupBy preserves first-appearance order,
            // matching legacy's "print a subtotal whenever stock_cd changes" behavior rather
            // than an alphabetical re-sort.
            var stockGroups = lines
                .GroupBy(l => l.StockCode)
                .Select(group =>
                {
                    var subtotal = group.Sum(l => l.Value);
                    var costPerGarment = styleQuantity == 0 ? 0 : subtotal / styleQuantity;
                    return new TrimSheetStockGroupServiceModel
                    {
                        StockCode = group.Key,
                        StockDescription = group.First().StockDescription,
                        SubtotalValue = subtotal,
                        CostPerGarment = costPerGarment,
                        PercentageOfUnitPrice = unitPrice == 0 ? 0 : (costPerGarment / unitPrice) * 100,
                    };
                })
                .ToList();

            // 4. Supplier value breakdown (tot_print's supplier summary table).
            var supplierTotals = lines
                .GroupBy(l => l.SupplierCode)
                .Select(g => new TrimSheetSupplierTotalServiceModel
                {
                    SupplierCode = g.Key,
                    SupplierName = g.First().SupplierName,
                    TotalValue = g.Sum(l => l.Value),
                })
                .ToList();

            var grandTotal = lines.Sum(l => l.Value);

            // 5. Estimated Profit - only computed when the controller says the caller is
            // authorized (mirrors legacy's access('trimprof')), so an unauthorized caller
            // never even receives these figures over the wire.
            TrimSheetProfitServiceModel? profit = null;
            if (includeProfit)
            {
                var costPerGarment = styleQuantity == 0 ? 0 : grandTotal / styleQuantity;
                profit = new TrimSheetProfitServiceModel
                {
                    UnitPricePerGarment = unitPrice,
                    CostPerGarment = costPerGarment,
                    CostPercentageOfUnitPrice = unitPrice == 0 ? 0 : (costPerGarment / unitPrice) * 100,
                    EstimatedProfitPerGarment = unitPrice - costPerGarment,
                    EstimatedProfitPercentage = unitPrice == 0 ? 0 : ((unitPrice - costPerGarment) / unitPrice) * 100,
                };
            }

            // 6. Approval stamp - reuses the same centralized gate-check every other
            // approval-aware screen in this app already relies on.
            TrimSheetApprovalStampServiceModel? approvalStamp = null;
            var approvalDetails = await _styleApprovalService.GetStyleApprovalDetailsAsync(buyerCode, order, typeCode, styleCode);
            if (approvalDetails != null)
            {
                approvalStamp = new TrimSheetApprovalStampServiceModel
                {
                    ApprovedByUserId = approvalDetails.EstimateApprovalUserName ?? "",
                    ApprovedDate = approvalDetails.EstimateApprovalDate ?? default,
                };
            }

            return new TrimSheetReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyerName,
                Order = order,
                TypeCode = typeCode,
                TypeName = typeName,
                StyleCode = styleCode,
                Unit = styleRow.Unit ?? "",
                StyleQuantity = styleQuantity,
                UnitPrice = unitPrice,
                BasisCode = purchaseOrder.BasisCode ?? "",
                BasisDescription = basisDescription,
                CurrencyCode = orderCurrency,
                Lines = lines,
                StockGroupSubtotals = stockGroups,
                SupplierTotals = supplierTotals,
                GrandTotalValue = grandTotal,
                SubContractSectionAvailable = false,
                ProductionLineSectionAvailable = false,
                Profit = profit,
                ApprovalStamp = approvalStamp,
            };
        }
    }
}
