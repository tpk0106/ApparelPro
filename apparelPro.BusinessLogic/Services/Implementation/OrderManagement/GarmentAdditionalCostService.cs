using apparelPro.BusinessLogic.Services.Models.OrderManagement.IGarmentAdditionalCostService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class GarmentAdditionalCostService : IGarmentAdditionalCostService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public GarmentAdditionalCostService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
        }

        // Composes the 22-char composite ItemCode: StockCode(2) + ItemCode(4) + Feature1-4(4 each) -
        // same convention as StyleMaterialCostProfile.ItemCode (see MaterialConsumptionService's
        // ComposeCostProfileItemCode, duplicated locally here to keep this service self-contained).
        private static string ComposeItemCode(string stockCode, string itemCode, string feature1, string feature2, string feature3, string feature4)
        {
            static string Segment(string? value, int width) => (value ?? string.Empty).Trim().PadRight(width).Substring(0, width);
            return Segment(stockCode, 2) + Segment(itemCode, 4) + Segment(feature1, 4) + Segment(feature2, 4) + Segment(feature3, 4) + Segment(feature4, 4);
        }

        private static (string StockCode, string ItemCode, string Feature1, string Feature2, string Feature3, string Feature4) ParseItemCode(string compositeItemCode)
        {
            string padded = (compositeItemCode ?? string.Empty).PadRight(22).Substring(0, 22);
            return (
                padded.Substring(0, 2).TrimEnd(),
                padded.Substring(2, 4).TrimEnd(),
                padded.Substring(6, 4).TrimEnd(),
                padded.Substring(10, 4).TrimEnd(),
                padded.Substring(14, 4).TrimEnd(),
                padded.Substring(18, 4).TrimEnd()
            );
        }

        // Same "how many garments does this Colour/Size scope cover" logic as
        // MaterialConsumptionService.CalculateMaterialConsumptionAsync's Clipper Case Allocation
        // Selector block - duplicated here since Additional Cost has no unit-conversion step
        // (that method's Steps A/D don't apply), so it isn't the same calculation end-to-end.
        private async Task<decimal> GetTargetedGarmentCountAsync(int buyerCode, string order, int typeCode, string styleCode, string? color, string? size)
        {
            string? searchColor = color?.Trim();
            string? searchSize = size?.Trim();
            bool hasColor = !string.IsNullOrWhiteSpace(searchColor);
            bool hasSize = !string.IsNullOrWhiteSpace(searchSize);

            if (!hasColor && !hasSize)
            {
                var parentStyle = await _apparelProDbContext.Styles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order && s.TypeCode == typeCode && s.StyleCode == styleCode);
                return parentStyle?.Quantity ?? 0;
            }
            if (hasColor && !hasSize)
            {
                return await _apparelProDbContext.ColorSizeDetails
                    .AsNoTracking()
                    .Where(d => d.BuyerCode == buyerCode && d.Order == order && d.TypeCode == typeCode && d.StyleCode == styleCode && d.Color == searchColor)
                    .SumAsync(d => d.Qty);
            }
            if (!hasColor && hasSize)
            {
                return await _apparelProDbContext.ColorSizeDetails
                    .AsNoTracking()
                    .Where(d => d.BuyerCode == buyerCode && d.Order == order && d.TypeCode == typeCode && d.StyleCode == styleCode && d.Size == searchSize)
                    .SumAsync(d => d.Qty);
            }

            var cellRecord = await _apparelProDbContext.ColorSizeDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.BuyerCode == buyerCode && d.Order == order && d.TypeCode == typeCode && d.StyleCode == styleCode && d.Color == searchColor && d.Size == searchSize);
            return cellRecord?.Qty ?? 0;
        }

        public async Task<List<GarmentAdditionalCostServiceModel>> GetGarmentAdditionalCostsAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            var rows = await _apparelProDbContext.GarmentAdditionalCosts
                .AsNoTracking()
                .Where(g => g.BuyerCode == buyerCode && g.Order == order.Trim() && g.TypeCode == typeCode && g.StyleCode == styleCode.Trim())
                .ToListAsync();

            if (rows.Count == 0)
            {
                return new List<GarmentAdditionalCostServiceModel>();
            }

            var additionalCostCodes = rows.Select(r => r.AdditionalCostCode).Distinct().ToList();
            var additionalCostNames = await _apparelProDbContext.AdditionalCosts
                .Where(a => additionalCostCodes.Contains(a.Code))
                .ToDictionaryAsync(a => a.Code, a => a.Description);

            var storeCodes = rows.Select(r => r.StoreCode).Distinct().ToList();
            var storeNames = await _apparelProDbContext.Basis
                .Where(b => storeCodes.Contains(b.Code))
                .ToDictionaryAsync(b => b.Code, b => b.Description);

            // Same lookup GetGarmentAdditionalCostReportAsync already does - the list view
            // needs the Description too (it lives on StyleMaterialCostProfile, not on the
            // GarmentAdditionalCost row itself), otherwise the entry grid has no way to show
            // what an item actually is beyond its raw composite ItemCode.
            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var costProfileDescriptions = await _apparelProDbContext.StyleMaterialCostProfiles
                .Where(p => p.BuyerCode == buyerCode && p.Order == order.Trim() && p.TypeCode == typeCode && p.StyleCode == styleCode.Trim() && itemCodes.Contains(p.ItemCode))
                .ToDictionaryAsync(p => p.ItemCode, p => p.Description);

            var result = new List<GarmentAdditionalCostServiceModel>();
            foreach (var row in rows)
            {
                var (stockCode, itemCode, feature1, feature2, feature3, feature4) = ParseItemCode(row.ItemCode);
                result.Add(new GarmentAdditionalCostServiceModel
                {
                    BuyerCode = row.BuyerCode,
                    Order = row.Order,
                    TypeCode = row.TypeCode,
                    StyleCode = row.StyleCode,
                    AdditionalCostCode = row.AdditionalCostCode,
                    AdditionalCostName = additionalCostNames.TryGetValue(row.AdditionalCostCode, out var costName) ? costName : row.AdditionalCostCode,
                    Description = costProfileDescriptions.TryGetValue(row.ItemCode, out var desc) ? desc : "",
                    StockCode = stockCode,
                    ItemCode = itemCode,
                    Feature1 = feature1,
                    Feature2 = feature2,
                    Feature3 = feature3,
                    Feature4 = feature4,
                    Color = row.Color,
                    Size = row.Size,
                    StoreCode = row.StoreCode,
                    StoreName = storeNames.TryGetValue(row.StoreCode, out var storeName) ? storeName : row.StoreCode,
                    Currency = row.Currency,
                    Unit = row.Unit,
                    Quantity = row.Quantity,
                    Cost = row.Cost,
                    IsCostPerGarment = row.IsCostPerGarment,
                    IsSemiFinishedGarment = row.IsSemiFinishedGarment
                });
            }

            return result;
        }

        public async Task<GarmentAdditionalCostServiceModel> SaveGarmentAdditionalCostAsync(SaveGarmentAdditionalCostServiceModel request)
        {
            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string order = request.Order.Trim();
                string styleCode = request.StyleCode.Trim();
                string additionalCostCode = request.AdditionalCostCode.Trim();
                string stockCode = request.StockCode.Trim();
                string itemCode4 = request.ItemCode.Trim();
                string feature1 = (request.Feature1 ?? "").Trim();
                string feature2 = (request.Feature2 ?? "").Trim();
                string feature3 = (request.Feature3 ?? "").Trim();
                string feature4 = (request.Feature4 ?? "").Trim();
                string color = (request.Color ?? "").Trim();
                string size = (request.Size ?? "").Trim();
                string storeCode = request.StoreCode.Trim();
                string currency = request.Currency.Trim();
                string unit = request.Unit.Trim();

                // --- Validation (Zero-Assumption boundary: every FK must exist, matching the
                // [F1]-Help lookups OD_AITM1.PRG performs before it lets the record be saved) ---
                var styleExists = await _apparelProDbContext.Styles
                    .AnyAsync(s => s.BuyerCode == request.BuyerCode && s.Order == order && s.TypeCode == request.TypeCode && s.StyleCode == styleCode);
                if (!styleExists)
                {
                    throw new InvalidOperationException("Invalid Buyer/Order/Type/Style.");
                }

                var additionalCostExists = await _apparelProDbContext.AdditionalCosts.AnyAsync(a => a.Code == additionalCostCode);
                if (!additionalCostExists)
                {
                    throw new InvalidOperationException("Invalid Additional Cost Code.");
                }

                var stockItemExists = await _apparelProDbContext.StockItems.AnyAsync(si => si.StockCode == stockCode && si.ItemCode == itemCode4);
                if (!stockItemExists)
                {
                    throw new InvalidOperationException("Invalid Stock/Item Code.");
                }

                var featureCodes = new[] { feature1, feature2, feature3, feature4 }.Where(f => !string.IsNullOrEmpty(f)).Distinct().ToList();
                if (featureCodes.Count > 0)
                {
                    var existingFeatureCount = await _apparelProDbContext.ItemFeatures.CountAsync(f => featureCodes.Contains(f.FeatureCode));
                    if (existingFeatureCount != featureCodes.Count)
                    {
                        throw new InvalidOperationException("One or more Feature codes do not exist in the Item Feature master.");
                    }
                }

                if (!string.IsNullOrEmpty(color))
                {
                    var colorExists = await _apparelProDbContext.ColorSizeDetails
                        .AnyAsync(d => d.BuyerCode == request.BuyerCode && d.Order == order && d.TypeCode == request.TypeCode && d.StyleCode == styleCode && d.Color == color);
                    if (!colorExists)
                    {
                        throw new InvalidOperationException("Invalid Colour Code.");
                    }
                }

                if (!string.IsNullOrEmpty(size))
                {
                    var sizeExists = await _apparelProDbContext.ColorSizeDetails
                        .AnyAsync(d => d.BuyerCode == request.BuyerCode && d.Order == order && d.TypeCode == request.TypeCode && d.StyleCode == styleCode && d.Size == size);
                    if (!sizeExists)
                    {
                        throw new InvalidOperationException("Invalid Size Code.");
                    }
                }

                var unitExists = await _apparelProDbContext.Units.AnyAsync(u => u.Code == unit);
                if (!unitExists)
                {
                    throw new InvalidOperationException("Invalid Unit Code.");
                }

                var basisExists = await _apparelProDbContext.Basis.AnyAsync(b => b.Code == storeCode);
                if (!basisExists)
                {
                    throw new InvalidOperationException("Invalid Basis Code.");
                }

                var currencyExists = await _apparelProDbContext.Currencies.AnyAsync(c => c.Code == currency);
                if (!currencyExists)
                {
                    throw new InvalidOperationException("Invalid Currency Code.");
                }

                if (request.Quantity <= 0)
                {
                    throw new InvalidOperationException("Quantity per Garment must be greater than zero.");
                }
                if (request.Cost <= 0)
                {
                    throw new InvalidOperationException("Cost must be greater than zero.");
                }

                string compositeItemCode = ComposeItemCode(stockCode, itemCode4, feature1, feature2, feature3, feature4);

                decimal targetedGarmentCount = await GetTargetedGarmentCountAsync(request.BuyerCode, order, request.TypeCode, styleCode, color, size);
                decimal totalConsumption = Math.Ceiling(request.Quantity * targetedGarmentCount);
                decimal unitPrice = request.IsCostPerGarment
                    ? request.Cost
                    : (totalConsumption > 0 ? request.Cost / totalConsumption : 0);

                // --- Upsert GarmentAdditionalCost ---
                decimal historicalConsumptionDelta = 0;
                var existingGarmentCost = await _apparelProDbContext.GarmentAdditionalCosts
                    .FirstOrDefaultAsync(g => g.BuyerCode == request.BuyerCode && g.Order == order && g.TypeCode == request.TypeCode &&
                                               g.StyleCode == styleCode && g.AdditionalCostCode == additionalCostCode && g.ItemCode == compositeItemCode);

                if (existingGarmentCost != null)
                {
                    var (existingStockCode, existingItemCode4, existingFeature1, existingFeature2, existingFeature3, existingFeature4) = ParseItemCode(existingGarmentCost.ItemCode);
                    var oldLedgerRow = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                        .FirstOrDefaultAsync(l => l.BuyerCode == request.BuyerCode && l.Order == order && l.TypeCode == request.TypeCode && l.StyleCode == styleCode &&
                                                   l.Color == existingGarmentCost.Color && l.Size == existingGarmentCost.Size &&
                                                   l.StockCode == existingStockCode && l.ItemCode == existingItemCode4 &&
                                                   l.Feature1 == existingFeature1 && l.Feature2 == existingFeature2 && l.Feature3 == existingFeature3 && l.Feature4 == existingFeature4);
                    if (oldLedgerRow != null)
                    {
                        historicalConsumptionDelta = oldLedgerRow.TotalConsumption;
                    }

                    existingGarmentCost.Color = color;
                    existingGarmentCost.Size = size;
                    existingGarmentCost.StoreCode = storeCode;
                    existingGarmentCost.Currency = currency;
                    existingGarmentCost.Unit = unit;
                    existingGarmentCost.Quantity = request.Quantity;
                    existingGarmentCost.Cost = request.Cost;
                    existingGarmentCost.IsCostPerGarment = request.IsCostPerGarment;
                    existingGarmentCost.IsSemiFinishedGarment = request.IsSemiFinishedGarment;
                    _apparelProDbContext.GarmentAdditionalCosts.Update(existingGarmentCost);
                }
                else
                {
                    _apparelProDbContext.GarmentAdditionalCosts.Add(new GarmentAdditionalCost
                    {
                        BuyerCode = request.BuyerCode,
                        Order = order,
                        TypeCode = request.TypeCode,
                        StyleCode = styleCode,
                        AdditionalCostCode = additionalCostCode,
                        ItemCode = compositeItemCode,
                        Color = color,
                        Size = size,
                        StoreCode = storeCode,
                        Currency = currency,
                        Unit = unit,
                        Quantity = request.Quantity,
                        Cost = request.Cost,
                        IsCostPerGarment = request.IsCostPerGarment,
                        IsSemiFinishedGarment = request.IsSemiFinishedGarment
                    });
                }

                // --- Upsert StyleMaterialCostProfile (same historical-delta netting SaveMaterialConsumptionEntryAsync uses) ---
                var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                    .FirstOrDefaultAsync(p => p.BuyerCode == request.BuyerCode && p.Order == order && p.TypeCode == request.TypeCode &&
                                               p.StyleCode == styleCode && p.ItemCode == compositeItemCode);
                if (costProfile != null)
                {
                    costProfile.BalanceQuantity = (costProfile.BalanceQuantity - historicalConsumptionDelta) + totalConsumption;
                    costProfile.TotalConsumption = (costProfile.TotalConsumption - historicalConsumptionDelta) + totalConsumption;
                    costProfile.UnitPrice = unitPrice;
                    costProfile.Description = !string.IsNullOrWhiteSpace(request.Description) ? request.Description.Trim() : costProfile.Description;
                    costProfile.ItemUnit = unit;
                    costProfile.Currency = currency;
                    _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
                }
                else
                {
                    _apparelProDbContext.StyleMaterialCostProfiles.Add(new ApparelPro.Data.Models.OrderManagement.MaterialConsumption.StyleMaterialCostProfile
                    {
                        BuyerCode = request.BuyerCode,
                        Order = order,
                        TypeCode = request.TypeCode,
                        StyleCode = styleCode,
                        ItemCode = compositeItemCode,
                        Description = !string.IsNullOrWhiteSpace(request.Description) ? request.Description.Trim() : $"{stockCode}/{itemCode4} {feature1} {feature2} {feature3}".Trim(),
                        ItemUnit = unit,
                        Currency = currency,
                        UnitPrice = unitPrice,
                        BalanceQuantity = totalConsumption,
                        TotalConsumption = totalConsumption,
                        SupplierCode = ""
                    });
                }

                // --- Upsert StyleMaterialConsumptionLedger, tagged IsAdditionalCost = true ---
                var ledgerRow = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                    .FirstOrDefaultAsync(l => l.BuyerCode == request.BuyerCode && l.Order == order && l.TypeCode == request.TypeCode && l.StyleCode == styleCode &&
                                               l.Color == color && l.Size == size && l.StockCode == stockCode && l.ItemCode == itemCode4 &&
                                               l.Feature1 == feature1 && l.Feature2 == feature2 && l.Feature3 == feature3 && l.Feature4 == feature4);
                if (ledgerRow != null)
                {
                    ledgerRow.StoreCode = storeCode;
                    ledgerRow.ConsumptionUnit = unit;
                    ledgerRow.ItemUnit = unit;
                    ledgerRow.QuantityPerGarment = request.Quantity;
                    ledgerRow.TotalConsumption = totalConsumption;
                    ledgerRow.PercentageAllowance = 0;
                    ledgerRow.IsAdditionalCost = true;
                    ledgerRow.CalculateConsumption = true;
                    _apparelProDbContext.StyleMaterialConsumptionLedgers.Update(ledgerRow);
                }
                else
                {
                    _apparelProDbContext.StyleMaterialConsumptionLedgers.Add(new ApparelPro.Data.Models.OrderManagement.MaterialConsumption.StyleMaterialConsumptionLedger
                    {
                        BuyerCode = request.BuyerCode,
                        Order = order,
                        TypeCode = request.TypeCode,
                        StyleCode = styleCode,
                        Color = color,
                        Size = size,
                        StockCode = stockCode,
                        ItemCode = itemCode4,
                        Feature1 = feature1,
                        Feature2 = feature2,
                        Feature3 = feature3,
                        Feature4 = feature4,
                        StoreCode = storeCode,
                        ConsumptionUnit = unit,
                        ItemUnit = unit,
                        QuantityPerGarment = request.Quantity,
                        SupplierCode = "",
                        TotalConsumption = totalConsumption,
                        PercentageAllowance = 0,
                        IsAdditionalCost = true,
                        CalculateConsumption = true
                    });
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                var additionalCostName = (await _apparelProDbContext.AdditionalCosts.FirstOrDefaultAsync(a => a.Code == additionalCostCode))?.Description ?? additionalCostCode;
                var storeName = (await _apparelProDbContext.Basis.FirstOrDefaultAsync(b => b.Code == storeCode))?.Description ?? storeCode;

                return new GarmentAdditionalCostServiceModel
                {
                    BuyerCode = request.BuyerCode,
                    Order = order,
                    TypeCode = request.TypeCode,
                    StyleCode = styleCode,
                    AdditionalCostCode = additionalCostCode,
                    AdditionalCostName = additionalCostName,
                    StockCode = stockCode,
                    ItemCode = itemCode4,
                    Feature1 = feature1,
                    Feature2 = feature2,
                    Feature3 = feature3,
                    Feature4 = feature4,
                    Color = color,
                    Size = size,
                    StoreCode = storeCode,
                    StoreName = storeName,
                    Currency = currency,
                    Unit = unit,
                    Quantity = request.Quantity,
                    Cost = request.Cost,
                    IsCostPerGarment = request.IsCostPerGarment,
                    IsSemiFinishedGarment = request.IsSemiFinishedGarment
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteGarmentAdditionalCostAsync(int buyerCode, string order, int typeCode, string styleCode, string additionalCostCode, string itemCode)
        {
            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string orderClean = order.Trim();
                string styleClean = styleCode.Trim();
                string additionalCostClean = additionalCostCode.Trim();
                string itemCodeClean = itemCode.Trim();

                var garmentCost = await _apparelProDbContext.GarmentAdditionalCosts
                    .FirstOrDefaultAsync(g => g.BuyerCode == buyerCode && g.Order == orderClean && g.TypeCode == typeCode &&
                                               g.StyleCode == styleClean && g.AdditionalCostCode == additionalCostClean && g.ItemCode == itemCodeClean);
                if (garmentCost == null)
                {
                    return true; // already gone
                }

                // SUPPLIER PO LOCK GUARD - same check DeleteConsumptionEntryAsync uses: block only if
                // a supplier PO has actually been raised against this specific composite ItemCode.
                var isPoRaised = await _apparelProDbContext.SupplierPurchaseOrderDetails
                    .AnyAsync(d => d.Buyer == buyerCode && d.Order == orderClean && d.Type == typeCode && d.Style == styleClean && d.ItemCode == itemCodeClean);
                if (isPoRaised)
                {
                    return false;
                }

                var (stockCode, itemCode4, feature1, feature2, feature3, feature4) = ParseItemCode(garmentCost.ItemCode);

                var ledgerRow = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                    .FirstOrDefaultAsync(l => l.BuyerCode == buyerCode && l.Order == orderClean && l.TypeCode == typeCode && l.StyleCode == styleClean &&
                                               l.Color == garmentCost.Color && l.Size == garmentCost.Size && l.StockCode == stockCode && l.ItemCode == itemCode4 &&
                                               l.Feature1 == feature1 && l.Feature2 == feature2 && l.Feature3 == feature3 && l.Feature4 == feature4);

                if (ledgerRow != null)
                {
                    var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                        .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode && p.Order == orderClean && p.TypeCode == typeCode &&
                                                   p.StyleCode == styleClean && p.ItemCode == itemCodeClean);
                    if (costProfile != null)
                    {
                        costProfile.BalanceQuantity -= ledgerRow.TotalConsumption;
                        if (costProfile.BalanceQuantity <= 0)
                        {
                            _apparelProDbContext.StyleMaterialCostProfiles.Remove(costProfile);
                        }
                        else
                        {
                            _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
                        }
                    }

                    _apparelProDbContext.StyleMaterialConsumptionLedgers.Remove(ledgerRow);
                }

                _apparelProDbContext.GarmentAdditionalCosts.Remove(garmentCost);

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return true;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<GarmentAdditionalCostReportServiceModel> GetGarmentAdditionalCostReportAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            string orderClean = order.Trim();
            string styleClean = styleCode.Trim();

            var rows = await _apparelProDbContext.GarmentAdditionalCosts
                .AsNoTracking()
                .Where(g => g.BuyerCode == buyerCode && g.Order == orderClean && g.TypeCode == typeCode && g.StyleCode == styleClean)
                .ToListAsync();

            var report = new GarmentAdditionalCostReportServiceModel
            {
                BuyerCode = buyerCode,
                Order = orderClean,
                TypeCode = typeCode,
                StyleCode = styleClean
            };

            if (rows.Count == 0)
            {
                return report;
            }

            var additionalCostCodes = rows.Select(r => r.AdditionalCostCode).Distinct().ToList();
            var additionalCostNames = await _apparelProDbContext.AdditionalCosts
                .Where(a => additionalCostCodes.Contains(a.Code))
                .ToDictionaryAsync(a => a.Code, a => a.Description);

            var storeCodes = rows.Select(r => r.StoreCode).Distinct().ToList();
            var storeNames = await _apparelProDbContext.Basis
                .Where(b => storeCodes.Contains(b.Code))
                .ToDictionaryAsync(b => b.Code, b => b.Description);

            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var costProfileDescriptions = await _apparelProDbContext.StyleMaterialCostProfiles
                .Where(p => p.BuyerCode == buyerCode && p.Order == orderClean && p.TypeCode == typeCode && p.StyleCode == styleClean && itemCodes.Contains(p.ItemCode))
                .ToDictionaryAsync(p => p.ItemCode, p => p.Description);

            foreach (var group in rows.GroupBy(r => r.AdditionalCostCode).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var category = new GarmentAdditionalCostCategoryServiceModel
                {
                    AdditionalCostCode = group.Key,
                    AdditionalCostName = additionalCostNames.TryGetValue(group.Key, out var costName) ? costName : group.Key
                };

                foreach (var row in group)
                {
                    decimal targetedGarmentCount = await GetTargetedGarmentCountAsync(buyerCode, orderClean, typeCode, styleClean, row.Color, row.Size);

                    decimal price;
                    decimal value;
                    if (row.IsCostPerGarment)
                    {
                        price = row.Cost;
                        value = row.Cost * targetedGarmentCount;
                    }
                    else
                    {
                        value = row.Cost;
                        price = targetedGarmentCount > 0 ? row.Cost / targetedGarmentCount : 0;
                    }

                    category.Lines.Add(new GarmentAdditionalCostLineServiceModel
                    {
                        ItemCode = row.ItemCode,
                        Description = costProfileDescriptions.TryGetValue(row.ItemCode, out var desc) ? desc : row.ItemCode,
                        Color = row.Color,
                        Size = row.Size,
                        Unit = row.Unit,
                        Quantity = row.Quantity,
                        StoreCode = row.StoreCode,
                        StoreName = storeNames.TryGetValue(row.StoreCode, out var storeName) ? storeName : row.StoreCode,
                        Currency = row.Currency,
                        Price = price,
                        Value = value
                    });
                    category.TotalValue += value;
                }

                report.Categories.Add(category);
            }

            return report;
        }
    }
}
