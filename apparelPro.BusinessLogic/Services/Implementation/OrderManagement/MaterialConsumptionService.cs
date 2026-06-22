using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.WebApi.APIModels.OrderManagement;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class MaterialConsumptionService : IMaterialConsumptionService
    {
        private readonly ApparelProDbContext _dbContext;
        private readonly IMapper _mapper;

        public MaterialConsumptionService(IMapper mapper, ApparelProDbContext dbContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        //public async Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync()
        //{
        //    var orderItemDbModelList = await _dbContext.OrderItems
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
            var featureMap = await _dbContext.OrderItemFeatures
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
                result.Feature1Type = (await _dbContext.ItemFeatures.FindAsync(featureMap.Feature1Type))?.Description;

            if (!string.IsNullOrEmpty(featureMap.Feature2Type))
                result.Feature2Type = (await _dbContext.ItemFeatures.FindAsync(featureMap.Feature2Type))?.Description;

            if (!string.IsNullOrEmpty(featureMap.Feature3Type))
                result.Feature3Type = (await _dbContext.ItemFeatures.FindAsync(featureMap.Feature3Type))?.Description;

            if (!string.IsNullOrEmpty(featureMap.Feature4Type))
                result.Feature4Type = (await _dbContext.ItemFeatures.FindAsync(featureMap.Feature4Type))?.Description;

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
            var forwardRule = await _dbContext.UnitConversion
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.FromUnit == fromUnit && u.ToUnit == toUnit);

            if (forwardRule != null && forwardRule.Measure.HasValue)
            {
                // Multiply when moving forward from Left to Right
                return quantity * forwardRule.Measure.Value;
            }

            // 2. Look for a Reverse Conversion rule (From Right to Left, e.g., PCS -> GRS)
            var reverseRule = await _dbContext.UnitConversion
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
                var parentStyle = await _dbContext.Styles
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
                targetedGarmentCount = await _dbContext.ColorSizeDetails
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
                targetedGarmentCount = await _dbContext.ColorSizeDetails
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
                var cellRecord = await _dbContext.ColorSizeDetails
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
        // REMOVED: context.Database.BeginTransactionAsync() to prevent MARS savepoint conflict errors!
        try
        {
            // ---------------------------------------------------------------------
            // PHASE 1: Process Granular Material Consumption Spreadsheet Entry
            // ---------------------------------------------------------------------

            var existingLedgerRow = await _dbContext.StyleMaterialConsumptionLedgers
                .FirstOrDefaultAsync(l => l.BuyerCode == request.BuyerCode &&
                                          l.Order == request.Order.Trim() &&
                                          l.TypeCode == request.TypeCode &&
                                          l.StyleCode == request.StyleCode.Trim() &&
                                          l.Color == request.Color.Trim() &&
                                          l.Size == request.Size.Trim() &&
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

                _dbContext.StyleMaterialConsumptionLedgers.Update(existingLedgerRow);
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
                    SupplierCode = request.SupplierCode.Trim(),
                    StoreCode = "", // need to check why request does not have store code 
                };

                await _dbContext.StyleMaterialConsumptionLedgers.AddAsync(newLedgerRow);
            }

            // ---------------------------------------------------------------------
            // PHASE 2: Adjust Consolidated Financial Cost Profile Matrix Balance
            // ---------------------------------------------------------------------

            var costProfile = await _dbContext.StyleMaterialCostProfiles
                .FirstOrDefaultAsync(p => p.BuyerCode == request.BuyerCode &&
                                          p.Order == request.Order.Trim() &&
                                          p.TypeCode == request.TypeCode &&
                                          p.StyleCode == request.StyleCode.Trim() &&
                                          p.StockCode == request.StockCode.Trim() &&
                                          p.ItemCode == request.ItemCode.Trim() &&
                                          p.Feature1 == request.Feature1.Trim() &&
                                          p.Feature2 == request.Feature2.Trim() &&
                                          p.Feature3 == request.Feature3.Trim() &&
                                          p.Feature4 == request.Feature4.Trim());

            if (costProfile != null)
            {
                costProfile.BalanceQuantity = (costProfile.BalanceQuantity - historicalConsumptionDelta) + request.TotalConsumption;
                costProfile.UnitPrice = request.UnitPrice;
                costProfile.Description = $"{request.StockCode}/{request.ItemCode} Component Entry Matched";
                costProfile.ItemUnit = request.ItemUnit.Trim();

                _dbContext.StyleMaterialCostProfiles.Update(costProfile);
            }
            else
            {
                    var newCostProfile = new StyleMaterialCostProfile
                    {
                        BuyerCode = request.BuyerCode,
                        Order = request.Order.Trim(),
                        TypeCode = request.TypeCode,
                        StyleCode = request.StyleCode.Trim(),
                        StockCode = request.StockCode.Trim(),
                        ItemCode = request.ItemCode.Trim(),
                        Feature1 = request.Feature1.Trim(),
                        Feature2 = request.Feature2.Trim(),
                        Feature3 = request.Feature3.Trim(),
                        Feature4 = request.Feature4.Trim(),
                        Description = $"{request.StockCode}/{request.ItemCode} Created Entry",
                        ItemUnit = request.ItemUnit.Trim(),

                        // FIXED: Maps your explicit runtime request parameter value to your SQL database column header!
                        Currency = request.Currency.Trim(),
                        UnitPrice = request.UnitPrice,
                        BalanceQuantity = request.TotalConsumption
                    };

                    await _dbContext.StyleMaterialCostProfiles.AddAsync(newCostProfile);
            }

            // 3. ATOMIC ENFORCEMENT: A single SaveChangesAsync call processes all track adjustments 
            // inside an implicit, isolated database transaction cleanly, safely supporting MARS connections.
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            // No manual rollback needed; SaveChangesAsync automatically aborts all pending track adjustments on error!
            throw;
        }
    }


    public async Task<bool> SaveMaterialConsumptionEntryAsync1(CreateMaterialConsumptionEntryRequestServiceModel request)
        {
            // Wrap both file adjustments inside a secure database transaction block(ACID compliance)
            using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 1: Handle Detailed Consumption Records Spreadsheet
                // ---------------------------------------------------------------------

                // Query if an identical intersection record already exists in the table layout
                var existingLedgerRow = await _dbContext.StyleMaterialConsumptionLedgers
                    .FirstOrDefaultAsync(l => l.BuyerCode == request.BuyerCode &&
                                              l.Order == request.Order.Trim() &&
                                              l.TypeCode == request.TypeCode &&
                                              l.StyleCode == request.StyleCode.Trim() &&
                                              l.Color == request.Color.Trim() &&
                                              l.Size == request.Size.Trim() &&
                                              l.StockCode == request.StockCode.Trim() &&
                                              l.ItemCode == request.ItemCode.Trim() &&
                                              l.Feature1 == request.Feature1.Trim() &&
                                              l.Feature2 == request.Feature2.Trim() &&
                                              l.Feature3 == request.Feature3.Trim() &&
                                              l.Feature4 == request.Feature4.Trim());

                decimal historicalConsumptionDelta = 0;

                if (existingLedgerRow != null)
                {
                    // Capture old totals before overwriting to calculate our financial adjustments
                    historicalConsumptionDelta = existingLedgerRow.TotalConsumption;

                    // Update existing cell properties directly
                    existingLedgerRow.QuantityPerGarment = request.QuantityPerGarment;
                    existingLedgerRow.PercentageAllowance = request.PercentageAllowance;
                    existingLedgerRow.TotalConsumption = request.TotalConsumption;
                    existingLedgerRow.SupplierCode = request.SupplierCode.Trim();
                    existingLedgerRow.ConsumptionUnit = request.ConsumptionUnit.Trim();
                    existingLedgerRow.ItemUnit = request.ItemUnit.Trim();

                    _dbContext.StyleMaterialConsumptionLedgers.Update(existingLedgerRow);
                }
                else
                {
                    // Insert a brand new granular matrix row
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

                    await _dbContext.StyleMaterialConsumptionLedgers.AddAsync(newLedgerRow);
                }

                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 2: Handle Consolidated Financial Cost Profiles
                // Replicates Clipper logic: repl tot_con with (tot_con - prev_qty) + new_qty
                // ---------------------------------------------------------------------

                var costProfile = await _dbContext.StyleMaterialCostProfiles
    .FirstOrDefaultAsync(p => p.BuyerCode == request.BuyerCode &&
                              p.Order == request.Order.Trim() &&
                              p.TypeCode == request.TypeCode &&
                              p.StyleCode == request.StyleCode.Trim() &&
                              p.StockCode == request.StockCode.Trim() &&
                              p.ItemCode == request.ItemCode.Trim() &&
                              p.Feature1 == request.Feature1.Trim() &&
                              p.Feature2 == request.Feature2.Trim() &&
                              p.Feature3 == request.Feature3.Trim() &&
                              p.Feature4 == request.Feature4.Trim());

                if (costProfile != null)
                {
                    // FIXED: Adjusted running totals using ONLY BalanceQuantity (matching your exact data model)
                    costProfile.BalanceQuantity = (costProfile.BalanceQuantity - historicalConsumptionDelta) + request.TotalConsumption;

                    // Sync unit pricing details dynamically
                    costProfile.UnitPrice = request.UnitPrice;
                    costProfile.Description = $"{request.StockCode}/{request.ItemCode} Component Entry Matched";
                    costProfile.ItemUnit = request.ItemUnit.Trim();

                    _dbContext.StyleMaterialCostProfiles.Update(costProfile);
                }
                else
                {
                    // Insert a fresh master financial header record using your exact properties
                    var newCostProfile = new StyleMaterialCostProfile
                    {
                        BuyerCode = request.BuyerCode,
                        Order = request.Order.Trim(),
                        TypeCode = request.TypeCode,
                        StyleCode = request.StyleCode.Trim(),
                        StockCode = request.StockCode.Trim(),
                        ItemCode = request.ItemCode.Trim(),
                        Feature1 = request.Feature1.Trim(),
                        Feature2 = request.Feature2.Trim(),
                        Feature3 = request.Feature3.Trim(),
                        Feature4 = request.Feature4.Trim(),
                        Description = $"{request.StockCode}/{request.ItemCode} Created Entry",
                        ItemUnit = request.ItemUnit.Trim(),
                        Currency = "USD", // System default, can be extended dynamically later
                        UnitPrice = request.UnitPrice,
                        BalanceQuantity = request.TotalConsumption // FIXED
                    };

                    await _dbContext.StyleMaterialCostProfiles.AddAsync(newCostProfile);
                }

                // Save modifications sequentially to SQL Server and commit transaction parameters cleanly
                await _dbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                // Roll back changes atomically if a database operation fails
                await dbTransaction.RollbackAsync();
                throw;
            }
        }


        public async Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync()
        {
            var orderItemDbModelList = await _dbContext.OrderItems
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

        public async Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync1(int buyerCode, string order, int typeCode, string styleCode)
        {
            order = order.Trim();
            styleCode = styleCode.Trim();

            // 1. Check if this Style already has allocated materials in the Ledger
            var hasExistingEntries = await _dbContext.StyleMaterialConsumptionLedgers
                .AnyAsync(l => l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode);

            // 2. If it is a brand-new Style entry session, run the Clipper Auto-Population Routine
            if (!hasExistingEntries)
            {
                // Fetch default material requirements mapped for this specific garment type template
                var templates = await _dbContext.GarmentTypeItems
                    .Where(t => t.GarmentTypeId == typeCode)
                    .ToListAsync();

                if (templates.Any())
                {
                    var initialLedgerRows = new List<StyleMaterialConsumptionLedger>();

                    foreach (var template in templates)
                    {
                        // Verify we don't accidentally create duplicates
                        bool exists = await _dbContext.StyleMaterialConsumptionLedgers.AnyAsync(l =>
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
                        await _dbContext.StyleMaterialConsumptionLedgers.AddRangeAsync(initialLedgerRows);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            // 3. Return the distinct list of materials allocated to this style to build the left panel checklist
            var materials = await _dbContext.StyleMaterialConsumptionLedgers
                .Where(l => l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode)
                .Select(l => new { l.StockCode, l.ItemCode })
                .Distinct()
                .ToListAsync();

            var resultList = new List<OrderItemServiceModel>();
            foreach (var mat in materials)
            {
                var masterItem = await _dbContext.OrderItems
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
            var hasExistingEntries = await _dbContext.StyleMaterialConsumptionLedgers
                .AnyAsync(l => l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode);

            // 2. If it is a brand-new editing session, pre-populate using the GarmentTypeTemplate bridge rows
            if (!hasExistingEntries)
            {
                // Query the dedicated bridge table using your independent typeCode identifier integer
                var templates = await _dbContext.GarmentTypeItems
                    .AsNoTracking()
                    .Where(t => t.GarmentTypeId == typeCode)
                    .ToListAsync();

                if (templates.Any())
                {
                    var initialLedgerRows = new List<StyleMaterialConsumptionLedger>();

                    foreach (var template in templates)
                    {
                        // Verify we don't write duplicate rows into the table
                        bool exists = await _dbContext.StyleMaterialConsumptionLedgers.AnyAsync(l =>
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
                        await _dbContext.StyleMaterialConsumptionLedgers.AddRangeAsync(initialLedgerRows);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            // 3. Extract the active checklist items assigned to this style from the Ledger table
            var assignedMaterials = await _dbContext.StyleMaterialConsumptionLedgers
                .Where(l => l.BuyerCode == buyerCode && l.Order == order && l.TypeCode == typeCode && l.StyleCode == styleCode)
                .Select(l => new { l.StockCode, l.ItemCode })
                .Distinct()
                .ToListAsync();

            var resultList = new List<OrderItemServiceModel>();

            // Cross-reference descriptions from your master material checklist entity
            foreach (var mat in assignedMaterials)
            {
                var masterItem = await _dbContext.OrderItems
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



        public async Task<List<StyleMaterialConsumptionLedger>> GetLedgerEntriesByStyleAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            return await _dbContext.StyleMaterialConsumptionLedgers
                .AsNoTracking()
                .Where(l => l.BuyerCode == buyerCode &&
                            l.Order == order.Trim() &&
                            l.TypeCode == typeCode &&
                            l.StyleCode == styleCode.Trim())
                .OrderBy(l => l.StockCode)
                .ThenBy(l => l.ItemCode)
                .ToListAsync();
        }


        public async Task<bool> DeleteConsumptionEntryAsync(
    int buyerCode, string order, int typeCode, string styleCode,
    string stockCode, string itemCode, string color, string size)
        {
            using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. GLOBAL PO LOCK GUARD: Verify if a contract has been locked down in the PurchaseOrders table
                var isPoRaised = await _dbContext.PurchaseOrders
                    .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order.Trim());

                if (isPoRaised)
                {
                    // Aborts execution immediately and alerts the UI that changes are blocked
                    return false;
                }

                // 2. Locate the specific targeted item inside your ledger spreadsheet matrix
                var ledgerItem = await _dbContext.StyleMaterialConsumptionLedgers
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
                var costProfile = await _dbContext.StyleMaterialCostProfiles
                    .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode &&
                                              p.Order == order.Trim() &&
                                              p.TypeCode == typeCode &&
                                              p.StyleCode == styleCode.Trim() &&
                                              p.StockCode == stockCode.Trim() &&
                                              p.ItemCode == itemCode.Trim() &&
                                              p.Feature1 == ledgerItem.Feature1 &&
                                              p.Feature2 == ledgerItem.Feature2 &&
                                              p.Feature3 == ledgerItem.Feature3 &&
                                              p.Feature4 == ledgerItem.Feature4);

                if (costProfile != null)
                {
                    costProfile.BalanceQuantity -= totalConsumptionToDeduct;

                    if (costProfile.BalanceQuantity <= 0)
                    {
                        _dbContext.StyleMaterialCostProfiles.Remove(costProfile); // Purge empty cost rows completely
                    }
                    else
                    {
                        _dbContext.StyleMaterialCostProfiles.Update(costProfile);
                    }
                }

                // 4. Wipe out the line record from the ledger spreadsheet table
                _dbContext.StyleMaterialConsumptionLedgers.Remove(ledgerItem);

                await _dbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }


        //public async Task<StyleDimensionsLookupServiceModel> GetStyleDimensionsAsync(int buyerCode, string order, int typeCode, string styleCode)
        //{
        //    order = order.Trim();
        //    styleCode = styleCode.Trim();

        //    // Query distinct colors mapped to this active style matrix workspace context
        //    var activeColors = await _dbContext.ColorSizeDetails
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
        //    var activeSizes = await _dbContext.ColorSizeDetails
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
        //    using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // 1. ANCHOR GUARD CHECK: Check if a Purchase Order has already been raised in your system
        //        // Maps exactly to Clipper's: seek ... if found() error "P.O already raised"
        //        var isPoRaised = await _dbContext.PurchaseOrders // Replace with your exact PO detail table if named differently
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
        //        var ledgerItem = await _dbContext.StyleMaterialConsumptionLedgers
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
        //        var costProfile = await _dbContext.StyleMaterialCostProfiles
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
        //                _dbContext.StyleMaterialCostProfiles.Remove(costProfile); // Purge empty cost lines completely
        //            }
        //            else
        //            {
        //                _dbContext.StyleMaterialCostProfiles.Update(costProfile);
        //            }
        //        }

        //        // 4. Wipe out the entry line row from the ledger table
        //        _dbContext.StyleMaterialConsumptionLedgers.Remove(ledgerItem);

        //        await _dbContext.SaveChangesAsync();
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
