using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using ApparelPro.WebApi.APIModels.OrderManagement;
using AutoMapper;
using Azure.Core;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class MaterialConsumptionService : IMaterialConsumptionService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IMapper _mapper;

        public MaterialConsumptionService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
            _mapper = mapper;
        }

        // Builds the 22-char composite ItemCode that StyleMaterialCostProfiles keys on
        // (StockCode 2 + ItemCode 4 + Feature1-4 x4), from the separate segments this
        // service still receives/stores everywhere else (request DTOs, StyleMaterialConsumptionLedger).
        // Each segment is right-padded with spaces and capped to its fixed width, matching the
        // real composite format confirmed against production data (e.g. "0202BTPLAS2HLS    BLUE").
        private static string ComposeCostProfileItemCode(string stockCode, string itemCode, string feature1, string feature2, string feature3, string feature4)
        {
            static string Segment(string? value, int width) => (value ?? string.Empty).Trim().PadRight(width).Substring(0, width);

            return Segment(stockCode, 2) + Segment(itemCode, 4) + Segment(feature1, 4) + Segment(feature2, 4) + Segment(feature3, 4) + Segment(feature4, 4);
        }

        //public async Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync()
        //{
        //    var orderItemDbModelList = await _apparelProDbContext.OrderItems
        //        .AsNoTracking()
        //        .Select(item => new OrderItem
        //        {
        //            StockCode = item.StockCode,
        //            ItemCode = item.ItemCode,
        //            Description = item.Description
        //        })
        //        .ToListAsync();
        //    var OrderItemServiceModelList = _mapper.Map<List<OrderItemServiceModel>>(orderItemDbModelList);
        //    return OrderItemServiceModelList;
        //}

        public async Task<OrderItemFeatureServiceModel?> GetDynamicFeatureHeadersAsync(string stockCode, string itemCode)
        {
            // 1. Find the target tracking rules configuration map for this specific material pair
            var featureMap = await _apparelProDbContext.OrderItemFeatures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.StockCode == stockCode && f.ItemCode == itemCode);

            if (featureMap == null) return null;

            // 2. Extract and match descriptions from your metadata table definitions via asynchronous lookups
            var result = new OrderItemFeature
            {
                StockCode = stockCode,
                ItemCode = itemCode,
                CostPerUnit = featureMap.CostPerUnit
            };

            if (!string.IsNullOrEmpty(featureMap.Feature1Type))
                result.Feature1Type = (await _apparelProDbContext.ItemFeatures.FindAsync(featureMap.Feature1Type))?.Description;

            if (!string.IsNullOrEmpty(featureMap.Feature2Type))
                result.Feature2Type = (await _apparelProDbContext.ItemFeatures.FindAsync(featureMap.Feature2Type))?.Description;

            if (!string.IsNullOrEmpty(featureMap.Feature3Type))
                result.Feature3Type = (await _apparelProDbContext.ItemFeatures.FindAsync(featureMap.Feature3Type))?.Description;

            if (!string.IsNullOrEmpty(featureMap.Feature4Type))
                result.Feature4Type = (await _apparelProDbContext.ItemFeatures.FindAsync(featureMap.Feature4Type))?.Description;

            var orderItemFeature = _mapper.Map<OrderItemFeatureServiceModel>(result);

            return orderItemFeature;
        }

        public async Task<decimal> ConvertUnitAsync(string fromUnit, string toUnit, decimal quantity)
        {
            fromUnit = fromUnit.Trim().ToUpper();
            toUnit = toUnit.Trim().ToUpper();

            // Base Case Guard: No calculation needed if the unit codes are identical
            if (fromUnit == toUnit) return quantity;

            // 1. Look for a Forward Conversion rule (From Left to Right, e.g., GRS -> PCS)
            var forwardRule = await _apparelProDbContext.UnitConversion
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.FromUnit == fromUnit && u.ToUnit == toUnit);

            if (forwardRule != null && forwardRule.Measure.HasValue)
            {
                // Multiply when moving forward from Left to Right
                return quantity * forwardRule.Measure.Value;
            }

            // 2. Look for a Reverse Conversion rule (From Right to Left, e.g., PCS -> GRS)
            var reverseRule = await _apparelProDbContext.UnitConversion
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.FromUnit == toUnit && u.ToUnit == fromUnit);

            if (reverseRule != null && reverseRule.Measure.HasValue)
            {
                // Safe Division Guard to prevent application crashes
                if (reverseRule.Measure.Value == 0) return 0;

                // Divide when moving backward from Right to Left
                return quantity / reverseRule.Measure.Value;
            }

            // 3. System Error Trap: Block execution if no relational mapping rule exists in the database
            throw new InvalidOperationException($"Unit Conversion Map Error: No conversion rule exists between unit code '{fromUnit}' and unit code '{toUnit}' inside the lookup tables.");
        }

        public async Task<decimal> CalculateMaterialConsumptionAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            string? garmentColor, string? garmentSize,
            string parentOrderUnit, string consumptionUnit, string finalItemUnit,
            decimal quantityPerGarment, decimal allowancePercentage)
        {
            decimal targetedGarmentCount = 0;

            // Sanitize search parameters to avoid whitespace mismatch gaps
            string? searchColor = garmentColor?.Trim();
            string? searchSize = garmentSize?.Trim();

            bool hasColor = !string.IsNullOrWhiteSpace(searchColor);
            bool hasSize = !string.IsNullOrWhiteSpace(searchSize);

            // --- 1. Clipper Case Allocation Selector Logic Blocks (Targeted Aggregates) ---
            if (!hasColor && !hasSize)
            {
                // Scenario 1: Universal Bulk Style Allocation (e.g. Care labels)
                var parentStyle = await _apparelProDbContext.Styles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode &&
                                              s.Order == order &&
                                              s.TypeCode == typeCode &&
                                              s.StyleCode == styleCode);

                targetedGarmentCount = parentStyle?.Quantity ?? 0;
            }
            else if (hasColor && !hasSize)
            {
                // Scenario 2: Color-Targeted Allocation (e.g. Matching Sewing Thread)
                targetedGarmentCount = await _apparelProDbContext.ColorSizeDetails
                    .AsNoTracking()
                    .Where(d => d.BuyerCode == buyerCode &&
                                d.Order == order &&
                                d.TypeCode == typeCode &&
                                d.StyleCode == styleCode &&
                                d.Color == searchColor)
                    .SumAsync(d => d.Qty);
            }
            else if (!hasColor && hasSize)
            {
                // Scenario 3: Size-Targeted Allocation (e.g. Branded Size Tags)
                targetedGarmentCount = await _apparelProDbContext.ColorSizeDetails
                    .AsNoTracking()
                    .Where(d => d.BuyerCode == buyerCode &&
                                d.Order == order &&
                                d.TypeCode == typeCode &&
                                d.StyleCode == styleCode &&
                                d.Size == searchSize)
                    .SumAsync(d => d.Qty);
            }
            else
            {
                // Scenario 4: Specific Micro-Intersection Block (e.g. Contrast panel fabrics)
                var cellRecord = await _apparelProDbContext.ColorSizeDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.BuyerCode == buyerCode &&
                                              d.Order == order &&
                                              d.TypeCode == typeCode &&
                                              d.StyleCode == styleCode &&
                                              d.Color == searchColor &&
                                              d.Size == searchSize);

                targetedGarmentCount = cellRecord?.Qty ?? 0;
            }

            // --- 2. Multi-Tier Generalized Unit Conversion & Material Math Matrix ---

            // Step A: Convert the source garment volume count from the Order Base Unit to the Consumption Unit scale
            // e.g. If order is in "DZ" (Dozens) but consumption is mapped in "PCS" (Pieces)
            decimal scaledGarmentVolume = await ConvertUnitAsync(parentOrderUnit, consumptionUnit, targetedGarmentCount);

            // Step B: Calculate net raw material requirements (Garments count * rate per individual garment)
            decimal netMaterialNeeded = quantityPerGarment * scaledGarmentVolume;

            // Step C: Compound manufacturing scrap waste allowance multiplier
            decimal grossCalculatedConsumption = netMaterialNeeded + ((netMaterialNeeded / 100m) * allowancePercentage);

            // Step D: Standardize the total consumption metrics to match the final Purchasing Unit used by suppliers
            // e.g. Converting required "PCS" up into "GRS" (Gross box counts)
            decimal totalFinalUnitConsumption = await ConvertUnitAsync(consumptionUnit, finalItemUnit, grossCalculatedConsumption);

            // Step E: Enforced Legacy Round-Up Ceiling Rule
            // If any fractional remainders exist, round up to the next full whole integer immediately
            return Math.Ceiling(totalFinalUnitConsumption);
        }



        public async Task<bool> SaveMaterialConsumptionEntryAsync(CreateMaterialConsumptionEntryRequestServiceModel request)
        {
            try
            {
                string itemCodeClean = request.ItemCode.Trim();
                string stockCodeClean = request.StockCode.Trim();

                // ---------------------------------------------------------------------
                // STEP 1: Update Global Reference Catalog (No StoreCode Needed!)
                // ---------------------------------------------------------------------
                var itemExistsInGlobalCatalog = await _apparelProDbContext.StockItems
                    .AnyAsync(c => c.StockCode == stockCodeClean && c.ItemCode == itemCodeClean);

                if (!itemExistsInGlobalCatalog)
                {
                    var globalCatalogItem = new StockItem
                    {
                        StockCode = stockCodeClean, // e.g., "02"
                        ItemCode = itemCodeClean,    // e.g., "0202BTA-PLAS4HOL24LNV"
                        Description = request.Description?.Trim().ToUpper() ?? $"{stockCodeClean}/{itemCodeClean} Component"
                    };
                    await _apparelProDbContext.StockItems.AddAsync(globalCatalogItem);
                    await _apparelProDbContext.SaveChangesAsync();
                }

                // ---------------------------------------------------------------------
                // PHASE 1: Process Granular Material Consumption Spreadsheet Entry
                // ---------------------------------------------------------------------
                // No store/warehouse concept here, by design — verified against the legacy
                // OD_TPDT1.PRG source: material consumption entry only writes od_sacc2/od_sacc3
                // (this ledger + the cost profile below), which is pure BOM planning. Store codes
                // are assigned later, at goods-received time, from od_stref (a plain code/description
                // lookup table with no auto-routing rules — there was never a legacy rule mapping a
                // stock category like "02" to a specific store).

                var existingLedgerRow = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                    .FirstOrDefaultAsync(l => l.BuyerCode == request.BuyerCode &&
                                              l.Order == request.Order.Trim() &&
                                              l.TypeCode == request.TypeCode &&
                                              l.StyleCode == request.StyleCode.Trim() &&
                                              l.StockCode == request.StockCode.Trim() &&
                                              l.ItemCode == request.ItemCode.Trim() &&
                                              l.Feature1 == request.Feature1.Trim() &&
                                              l.Feature2 == request.Feature2.Trim() &&
                                              l.Feature3 == request.Feature3.Trim() &&
                                              l.Feature4 == request.Feature4.Trim());

                decimal historicalConsumptionDelta = 0;

                if (existingLedgerRow != null)
                {
                    historicalConsumptionDelta = existingLedgerRow.TotalConsumption;

                    existingLedgerRow.QuantityPerGarment = request.QuantityPerGarment;
                    existingLedgerRow.PercentageAllowance = request.PercentageAllowance;
                    existingLedgerRow.TotalConsumption = request.TotalConsumption;
                    existingLedgerRow.SupplierCode = request.SupplierCode.Trim();
                    existingLedgerRow.ConsumptionUnit = request.ConsumptionUnit.Trim();
                    existingLedgerRow.ItemUnit = request.ItemUnit.Trim();

                    _apparelProDbContext.StyleMaterialConsumptionLedgers.Update(existingLedgerRow);
                }
                else
                {
                    var newLedgerRow = new StyleMaterialConsumptionLedger
                    {
                        BuyerCode = request.BuyerCode,
                        Order = request.Order.Trim(),
                        TypeCode = request.TypeCode,
                        StyleCode = request.StyleCode.Trim(),
                        Color = request.Color.Trim(),
                        Size = request.Size.Trim(),
                        StockCode = request.StockCode.Trim(),
                        ItemCode = request.ItemCode.Trim(),
                        Feature1 = request.Feature1.Trim(),
                        Feature2 = request.Feature2.Trim(),
                        Feature3 = request.Feature3.Trim(),
                        Feature4 = request.Feature4.Trim(),
                        ConsumptionUnit = request.ConsumptionUnit.Trim(),
                        ItemUnit = request.ItemUnit.Trim(),
                        QuantityPerGarment = request.QuantityPerGarment,
                        PercentageAllowance = request.PercentageAllowance,
                        TotalConsumption = request.TotalConsumption,
                        SupplierCode = request.SupplierCode.Trim()
                    };

                    await _apparelProDbContext.StyleMaterialConsumptionLedgers.AddAsync(newLedgerRow);
                }

                // ---------------------------------------------------------------------
                // PHASE 2: Adjust Consolidated Financial Cost Profile Matrix Balance
                // ---------------------------------------------------------------------

                // StyleMaterialCostProfiles now keys on a single 22-char composite ItemCode
                // (StockCode + ItemCode + Feature1-4), matching OrderwiseStockMaster/
                // OrderwiseStockTransaction/PODetails. StyleMaterialConsumptionLedger keeps its
                // separate segments (od_sacc3 legacy shape), so we compose the composite here
                // from the request's own separate fields before touching the cost profile.
                string compositeItemCode = ComposeCostProfileItemCode(
                    request.StockCode, request.ItemCode, request.Feature1, request.Feature2, request.Feature3, request.Feature4);

                var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                    .FirstOrDefaultAsync(p => p.BuyerCode == request.BuyerCode &&
                                              p.Order == request.Order.Trim() &&
                                              p.TypeCode == request.TypeCode &&
                                              p.StyleCode == request.StyleCode.Trim() &&
                                              p.ItemCode == compositeItemCode);

                if (costProfile != null)
                {
                    // BalanceQuantity (bal_qty) and TotalConsumption (tot_con) both move
                    // up/down together in response to a ledger entry change - they only
                    // diverge later, when a PO draws BalanceQuantity down without
                    // touching TotalConsumption (see SupplierPurchaseOrderService).
                    costProfile.BalanceQuantity = (costProfile.BalanceQuantity - historicalConsumptionDelta) + request.TotalConsumption;
                    costProfile.TotalConsumption = (costProfile.TotalConsumption - historicalConsumptionDelta) + request.TotalConsumption;
                    costProfile.UnitPrice = request.UnitPrice;
                    costProfile.Description = $"{request.StockCode}/{request.ItemCode} Component Entry Matched";
                    costProfile.ItemUnit = request.ItemUnit.Trim();
                    costProfile.SupplierCode = request.SupplierCode.Trim();

                    _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
                }
                else
                {
                    var newCostProfile = new StyleMaterialCostProfile
                    {
                        BuyerCode = request.BuyerCode,
                        Order = request.Order.Trim(),
                        TypeCode = request.TypeCode,
                        StyleCode = request.StyleCode.Trim(),
                        ItemCode = compositeItemCode,
                        Description = $"{request.StockCode}/{request.ItemCode} Created Entry",
                        ItemUnit = request.ItemUnit.Trim(),
                        Currency = request.Currency.Trim(),
                        UnitPrice = request.UnitPrice,
                        BalanceQuantity = request.TotalConsumption,
                        TotalConsumption = request.TotalConsumption,
                        SupplierCode = request.SupplierCode.Trim()
                    };

                    await _apparelProDbContext.StyleMaterialCostProfiles.AddAsync(newCostProfile);
                }

                // Removed PHASE 3 ("AUTOMATED ORDERWISE STOCK SYNCHRONIZATION"): it wrote
                // QtyInHand = TotalConsumption straight into OrderwiseStock, fabricating physical
                // stock the moment a BOM line was entered — before anything was purchased or
                // received. STRN/GIN draw down against OrderwiseStock.QtyInHand, so this could let
                // material be issued that was never actually received. OrderwiseStock is now
                // populated only by real receipt data: GRN (once built) or your DBF import,
                // matching the legacy flow exactly.

                await _apparelProDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }



        // NOTE (2026-07-22): SaveMaterialConsumptionEntryAsync2 and SaveMaterialConsumptionEntryAsync1
        // — two unreferenced duplicate drafts of the method below (neither is declared in
        // IMaterialConsumptionService, neither has any caller anywhere in the solution) — were
        // removed here. They still matched StyleMaterialCostProfiles by the old separate
        // StockCode/ItemCode/Feature1-4 columns, which no longer exist on that entity after the
        // 2026-07-22 collapse to a single 22-char composite ItemCode; keeping them would not compile.

        public async Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync()
        {
            var orderItemDbModelList = await _apparelProDbContext.OrderItems
                .AsNoTracking()
                .Select(item => new OrderItem
                {
                    StockCode = item.StockCode,
                    ItemCode = item.ItemCode,
                    Description = item.Description
                })
                .ToListAsync();

            // Maps your DB models directly to your clean Service Models for the UI list
            return _mapper.Map<List<OrderItemServiceModel>>(orderItemDbModelList);
        }

        public async Task<List<MaterialCatalogGroupServiceModel>> GetMaterialCatalogAsync()
        {
            // 1. Load every Stock category (parent rows) - always shown, even if empty
            var stocks = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .OrderBy(s => s.StockCode)
                .ToListAsync();

            // 2. Load the full item catalog and group in-memory by StockCode
            var items = await _apparelProDbContext.OrderItems
                .AsNoTracking()
                .OrderBy(i => i.ItemCode)
                .ToListAsync();

            var itemsByStock = items
                .GroupBy(i => i.StockCode)
                .ToDictionary(g => g.Key, g => g.ToList());

            return stocks.Select(s => new MaterialCatalogGroupServiceModel
            {
                StockCode = s.StockCode,
                Description = s.Description,
                Items = (itemsByStock.TryGetValue(s.StockCode, out var stockItems) ? stockItems : new List<OrderItem>())
                    .Select(i => new MaterialCatalogItemServiceModel
                    {
                        ItemCode = i.ItemCode,
                        Description = i.Description
                    })
                    .ToList()
            }).ToList();
        }

        public async Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync1(int buyerCode, string order, int typeCode, string styleCode)
        {
            order = order.Trim();
            styleCode = styleCode.Trim();

            // 1. Check if this Style already has allocated materials in the Ledger
            var hasExistingEntries = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                .AnyAsync(l => l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode);

            // 2. If it is a brand-new Style entry session, run the Clipper Auto-Population Routine
            if (!hasExistingEntries)
            {
                // Fetch default material requirements mapped for this specific garment type template
                var templates = await _apparelProDbContext.GarmentTypeItems
                    .Where(t => t.GarmentTypeId == typeCode)
                    .ToListAsync();

                if (templates.Any())
                {
                    var initialLedgerRows = new List<StyleMaterialConsumptionLedger>();

                    foreach (var template in templates)
                    {
                        // Verify we don't accidentally create duplicates
                        bool exists = await _apparelProDbContext.StyleMaterialConsumptionLedgers.AnyAsync(l =>
                            l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode &&
                            l.StockCode == template.StockCode && l.ItemCode == template.ItemCode);

                        if (!exists)
                        {
                            initialLedgerRows.Add(new StyleMaterialConsumptionLedger
                            {
                                BuyerCode = buyerCode,
                                Order = order,
                                TypeCode = typeCode,
                                StyleCode = styleCode,
                                StockCode = template.StockCode,
                                ItemCode = template.ItemCode,
                                ConsumptionUnit = template.Unit,
                                ItemUnit = template.Unit,
                                Color = "", // Standard global defaults
                                Size = "",
                                Feature1 = "",
                                Feature2 = "",
                                Feature3 = "",
                                Feature4 = "",
                                SupplierCode = ""
                            });
                        }
                    }

                    if (initialLedgerRows.Any())
                    {
                        await _apparelProDbContext.StyleMaterialConsumptionLedgers.AddRangeAsync(initialLedgerRows);
                        await _apparelProDbContext.SaveChangesAsync();
                    }
                }
            }

            // 3. Return the distinct list of materials allocated to this style to build the left panel checklist
            var materials = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                .Where(l => l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode)
                .Select(l => new { l.StockCode, l.ItemCode })
                .Distinct()
                .ToListAsync();

            var resultList = new List<OrderItemServiceModel>();
            foreach (var mat in materials)
            {
                var masterItem = await _apparelProDbContext.OrderItems
                    .FirstOrDefaultAsync(i => i.StockCode == mat.StockCode && i.ItemCode == mat.ItemCode);

                resultList.Add(new OrderItemServiceModel
                {
                    StockCode = mat.StockCode,
                    ItemCode = mat.ItemCode,
                    Description = masterItem?.Description ?? "Unknown Material Item"
                });
            }

            return resultList;
        }

        public async Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            order = order.Trim();
            styleCode = styleCode.Trim();

            // 1. Check if this specific style context already has recorded entries inside the Ledger table
            var hasExistingEntries = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                .AnyAsync(l => l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode);

            // 2. If it is a brand-new editing session, pre-populate using the GarmentTypeTemplate bridge rows
            if (!hasExistingEntries)
            {
                // Query the dedicated bridge table using your independent typeCode identifier integer
                var templates = await _apparelProDbContext.GarmentTypeItems
                    .AsNoTracking()
                    .Where(t => t.GarmentTypeId == typeCode)
                    .ToListAsync();

                if (templates.Any())
                {
                    var initialLedgerRows = new List<StyleMaterialConsumptionLedger>();

                    foreach (var template in templates)
                    {
                        // Verify we don't write duplicate rows into the table
                        bool exists = await _apparelProDbContext.StyleMaterialConsumptionLedgers.AnyAsync(l =>
                            l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode &&
                            l.StockCode == template.StockCode && l.ItemCode == template.ItemCode);

                        if (!exists)
                        {
                            initialLedgerRows.Add(new StyleMaterialConsumptionLedger
                            {
                                BuyerCode = buyerCode,
                                Order = order,
                                TypeCode = typeCode,
                                StyleCode = styleCode,
                                StockCode = template.StockCode,
                                ItemCode = template.ItemCode,
                                ConsumptionUnit = template.Unit,
                                ItemUnit = template.Unit,
                                Color = "", // Standard global blank spacers for new initializations
                                Size = "",
                                Feature1 = "",
                                Feature2 = "",
                                Feature3 = "",
                                Feature4 = "",
                                SupplierCode = "",
                                StoreCode = "",
                            });
                        }
                    }

                    if (initialLedgerRows.Any())
                    {
                        await _apparelProDbContext.StyleMaterialConsumptionLedgers.AddRangeAsync(initialLedgerRows);
                        await _apparelProDbContext.SaveChangesAsync();
                    }
                }
            }

            // 3. Extract the active checklist items assigned to this style from the Ledger table
            var assignedMaterials = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                .Where(l => l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode)
                .Select(l => new { l.StockCode, l.ItemCode })
                .Distinct()
                .ToListAsync();

            var resultList = new List<OrderItemServiceModel>();

            // Cross-reference descriptions from your master material checklist entity
            foreach (var mat in assignedMaterials)
            {
                var masterItem = await _apparelProDbContext.OrderItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.StockCode == mat.StockCode && i.ItemCode == mat.ItemCode);

                resultList.Add(new OrderItemServiceModel
                {
                    StockCode = mat.StockCode,
                    ItemCode = mat.ItemCode,
                    Description = masterItem?.Description ?? "Unregistered Material Name"
                });
            }

            return resultList;
        }

        public async Task<List<StyleMaterialConsumptionLedgerRowServiceModel>> GetLedgerEntriesByStyleAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            var ledgerRows = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                .AsNoTracking()
                .Where(l => l.BuyerCode == buyerCode &&
                            l.Order == order.Trim() &&
                            l.TypeCode == typeCode &&
                            l.StyleCode == styleCode.Trim())
                .OrderBy(l => l.StockCode)
                .ThenBy(l => l.ItemCode)
                .ToListAsync();

            if (ledgerRows.Count == 0)
                return new List<StyleMaterialConsumptionLedgerRowServiceModel>();

            // Bulk-fetch catalog descriptions once (small reference table), same
            // pattern used in GetMaterialCatalogAsync, instead of an N+1 lookup
            // per ledger row.
            var descriptionLookup = await _apparelProDbContext.OrderItems
                .AsNoTracking()
                .ToDictionaryAsync(i => (i.StockCode, i.ItemCode), i => i.Description);

            return ledgerRows.Select(l => new StyleMaterialConsumptionLedgerRowServiceModel
            {
                BuyerCode = l.BuyerCode,
                Order = l.Order,
                TypeCode = l.TypeCode,
                StyleCode = l.StyleCode,
                Color = l.Color,
                Size = l.Size,
                StockCode = l.StockCode,
                ItemCode = l.ItemCode,
                Description = descriptionLookup.TryGetValue((l.StockCode, l.ItemCode), out var desc)
                    ? desc
                    : $"{l.StockCode}/{l.ItemCode}",
                Feature1 = l.Feature1,
                Feature2 = l.Feature2,
                Feature3 = l.Feature3,
                Feature4 = l.Feature4,
                StoreCode = l.StoreCode,
                ConsumptionUnit = l.ConsumptionUnit,
                ItemUnit = l.ItemUnit,
                QuantityPerGarment = l.QuantityPerGarment,
                SupplierCode = l.SupplierCode,
                TotalConsumption = l.TotalConsumption,
                PercentageAllowance = l.PercentageAllowance,
                IsAdditionalCost = l.IsAdditionalCost,
                CalculateConsumption = l.CalculateConsumption,
            }).ToList();
        }


        public async Task<bool> DeleteConsumptionEntryAsync(int buyerCode, string order, int typeCode, string styleCode,
                string stockCode, string itemCode, string color, string size)
        {
            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. GLOBAL PO LOCK GUARD: Verify if a contract has been locked down in the PurchaseOrders table
                var isPoRaised = await _apparelProDbContext.PurchaseOrders
                    .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order.Trim());

                if (isPoRaised)
                {
                    // Aborts execution immediately and alerts the UI that changes are blocked
                    return false;
                }

                // 2. Locate the specific targeted item inside your ledger spreadsheet matrix
                var ledgerItem = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                    .FirstOrDefaultAsync(l => l.BuyerCode == buyerCode &&
                                              l.Order == order.Trim() &&
                                              l.TypeCode == typeCode &&
                                              l.StyleCode == styleCode.Trim() &&
                                              l.StockCode == stockCode.Trim() &&
                                              l.ItemCode == itemCode.Trim() &&
                                              l.Color == color.Trim() &&
                                              l.Size == size.Trim());

                if (ledgerItem == null) return true; // Record already deleted

                decimal totalConsumptionToDeduct = ledgerItem.TotalConsumption;

                // 3. FINANCIAL ADJUSTMENT: Deduct quantities from the consolidated Cost Profile
                // StyleMaterialCostProfiles now keys on the single 22-char composite ItemCode —
                // compose it from the segments this ledger row already carries.
                string compositeItemCode = ComposeCostProfileItemCode(
                    stockCode, itemCode, ledgerItem.Feature1, ledgerItem.Feature2, ledgerItem.Feature3, ledgerItem.Feature4);

                var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                    .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode &&
                                              p.Order == order.Trim() &&
                                              p.TypeCode == typeCode &&
                                              p.StyleCode == styleCode.Trim() &&
                                              p.ItemCode == compositeItemCode);

                if (costProfile != null)
                {
                    costProfile.BalanceQuantity -= totalConsumptionToDeduct;

                    if (costProfile.BalanceQuantity <= 0)
                    {
                        _apparelProDbContext.StyleMaterialCostProfiles.Remove(costProfile); // Purge empty cost rows completely
                    }
                    else
                    {
                        _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
                    }
                }

                // 4. Wipe out the line record from the ledger spreadsheet table
                _apparelProDbContext.StyleMaterialConsumptionLedgers.Remove(ledgerItem);

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        //public async Task IsMaterialConsumptionAllowedToEdit(int buyerCode, string order, int typeCode, string styleCode)
        //{
        //    // 1. QUERY THE STYLE MASTER HEADERS TO CHECK FOR ACTIVE MANAGERIAL APPROVALS
        //    var styleHeader = await _apparelProDbContext.Styles
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode &&
        //                                  s.Order == order.Trim() &&
        //                                  s.TypeCode == typeCode &&
        //                                  s.StyleCode == styleCode.Trim());

        //    if (styleHeader != null && styleHeader.ApprovedDate.HasValue &&
        //        styleHeader.ApprovedDate.Value != DateOnly.FromDateTime(DateTime.MinValue))
        //    {
        //        // 🚀 THE CRITICAL SECURITY GATE: If style is already approved, verify user privileges!
        //        // This mirrors Clipper's !access("cons_upd") validation check
        //        bool isHigherAuthority = User.IsInRole("Merchandising Manager") || User.IsInRole("Executive Director");

        //    }
        //}

        //public async Task<StyleApprovalDetailsServiceModel?> GetStyleApprovalDetailsAsync(int buyerCode, string order, int typeCode, string styleCode)
        //{
        //    // Query your master style headers table in SQL Server
        //    var styleHeader = await _apparelProDbContext.Styles
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode &&
        //                                  s.Order == order.Trim() &&
        //                                  s.TypeCode == typeCode &&
        //                                  s.StyleCode == styleCode.Trim());

        //    // FIXED VERIFICATION CONDITIONAL: Verify if an approval signature matches 
        //    if (styleHeader != null && styleHeader.ApprovedDate.HasValue &&
        //        styleHeader.ApprovedDate.Value != DateOnly.FromDateTime(DateTime.MinValue))
        //    {
        //        // Return the payload data structure straight to the controller layer
        //        return new StyleApprovalDetailsServiceModel
        //        {
        //            EstimateApprovalUserName = styleHeader.EstimateApprovalUserName ?? "SYSTEM_ADMIN",
        //            EstimateApprovalDate = styleHeader.ApprovedDate.Value
        //        };
        //    }

        //    // Return null to signal the controller that the material sheet is completely unlocked!
        //    return null;
        //}



        //public async Task<StyleDimensionsLookupServiceModel> GetStyleDimensionsAsync(int buyerCode, string order, int typeCode, string styleCode)
        //{
        //    order = order.Trim();
        //    styleCode = styleCode.Trim();

        //    // Query distinct colors mapped to this active style matrix workspace context
        //    var activeColors = await _apparelProDbContext.ColorSizeDetails
        //        .AsNoTracking()
        //        .Where(d => d.BuyerCode == buyerCode &&
        //                    d.Order == order &&
        //                    d.TypeCode == typeCode &&
        //                    d.StyleCode == styleCode &&
        //                    d.Color != null && d.Color != "")
        //        .Select(d => d.Color)
        //        .Distinct()
        //        .OrderBy(c => c)
        //        .ToListAsync();

        //    // Query distinct sizes mapped to this active style matrix workspace context
        //    var activeSizes = await _apparelProDbContext.ColorSizeDetails
        //        .AsNoTracking()
        //        .Where(d => d.BuyerCode == buyerCode &&
        //                    d.Order == order &&
        //                    d.TypeCode == typeCode &&
        //                    d.StyleCode == styleCode &&
        //                    d.Size != null && d.Size != "")
        //        .Select(d => d.Size)
        //        .Distinct()
        //        .OrderBy(s => s)
        //        .ToListAsync();

        //    return new StyleDimensionsLookupServiceModel
        //    {
        //        Colors = activeColors,
        //        Sizes = activeSizes
        //    };
        //}



        //public async Task<bool> DeleteConsumptionEntryAsync(int buyerCode, string order, int typeCode, string styleCode, string stockCode, string itemCode, string color, string size)
        //{
        //    using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // 1. ANCHOR GUARD CHECK: Check if a Purchase Order has already been raised in your system
        //        // Maps exactly to Clipper's: seek ... if found() error "P.O already raised"
        //        var isPoRaised = await _apparelProDbContext.PurchaseOrders // Replace with your exact PO detail table if named differently
        //            .AnyAsync(po => po.BuyerCode == buyerCode &&
        //                             po.Order == order.Trim()); //&&
        //                             //po.TypeCode == typeCode &&
        //                             //po.StyleCode == styleCode.Trim() &&
        //                             //po.StockCode == stockCode.Trim() &&
        //                             //po.ItemCode == itemCode.Trim());

        //        if (isPoRaised)
        //        {
        //            // Returns false to signify that a delete lock constraint was hit
        //            return false;
        //        }

        //        // 2. Locate the specific record inside your consumption ledger spreadsheet
        //        var ledgerItem = await _apparelProDbContext.StyleMaterialConsumptionLedgers
        //            .FirstOrDefaultAsync(l => l.BuyerCode == buyerCode &&
        //                                      l.Order == order.Trim() &&
        //                                      l.TypeCode == typeCode &&
        //                                      l.StyleCode == styleCode.Trim() &&
        //                                      l.StockCode == stockCode.Trim() &&
        //                                      l.ItemCode == itemCode.Trim() &&
        //                                      l.Color == color.Trim() &&
        //                                      l.Size == size.Trim());

        //        if (ledgerItem == null) return true; // Record already gone

        //        decimal totalConsumptionToDeduct = ledgerItem.TotalConsumption;

        //        // 3. TRANSACTION PHASE 2: Adjust your financial Cost Profile table totals downward
        //        var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
        //            .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode &&
        //                                      p.Order == order.Trim() &&
        //                                      p.TypeCode == typeCode &&
        //                                      p.StyleCode == styleCode.Trim() &&
        //                                      p.StockCode == stockCode.Trim() &&
        //                                      p.ItemCode == itemCode.Trim());

        //        if (costProfile != null)
        //        {
        //            costProfile.BalanceQuantity -= totalConsumptionToDeduct;

        //            if (costProfile.BalanceQuantity <= 0)
        //            {
        //                _apparelProDbContext.StyleMaterialCostProfiles.Remove(costProfile); // Purge empty cost lines completely
        //            }
        //            else
        //            {
        //                _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
        //            }
        //        }

        //        // 4. Wipe out the entry line row from the ledger table
        //        _apparelProDbContext.StyleMaterialConsumptionLedgers.Remove(ledgerItem);

        //        await _apparelProDbContext.SaveChangesAsync();
        //        await dbTransaction.CommitAsync();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        await dbTransaction.RollbackAsync();
        //        throw;
        //    }
        //}



    }
}
