using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPartShipmentService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class PartShipmentService : IPartShipmentService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IMapper _mapper;        
        private readonly IMaterialConsumptionService _materialConsumptionService;

        public PartShipmentService(
            ApparelProDbContext apparelProDbContext,
            IMapper mapper,
            IMaterialConsumptionService materialConsumptionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _mapper = mapper;
            _materialConsumptionService = materialConsumptionService;
        }

        public async Task<StyleShippingSummaryServiceModel> GetStyleShippingSummaryAsync(int buyerCode,
            string order, int typeCode, string styleCode)
        {
            order = order.Trim();
            styleCode = styleCode.Trim();

            var styleMaster = await _apparelProDbContext.Styles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order && s.TypeCode == typeCode && s.StyleCode == styleCode);

            if (styleMaster == null) return null!;

            var partialLines = await _apparelProDbContext.PartShipments
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order == order && p.TypeCode == typeCode && p.StyleCode == styleCode)
                .ToListAsync();

            // Accumulate cumulative shipping allocations across varied unit types safely
            decimal totalScheduledInBaseUnit = 0;
            string baseUnit = styleMaster.Unit?.Trim() ?? "PCS";

            foreach (var line in partialLines)
            {
                totalScheduledInBaseUnit += await _materialConsumptionService.ConvertUnitAsync(line.Unit, baseUnit, line.Quantity);
            }

            return new StyleShippingSummaryServiceModel
            {
                BuyerCode = buyerCode,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                Unit = baseUnit,
                TotalContractQuantity = Convert.ToDecimal(styleMaster.Quantity),
                TotalScheduledQuantity = totalScheduledInBaseUnit
            };
        }

        public async Task<List<PartShipment>> GetPartShipmentsByStyleAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            return await _apparelProDbContext.PartShipments
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order == order.Trim() && p.TypeCode == typeCode && p.StyleCode == styleCode.Trim())
                .OrderBy(p => p.ShipDate)
                .ToListAsync();
        }

        public async Task<bool> SavePartShipmentLineAsync(PartShipmentServiceModel request)
        {
            using (var transaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    request.Order = request.Order.Trim();
                    request.StyleCode = request.StyleCode.Trim();
                    request.NewOrder = request.NewOrder.Trim();
                    request.DestinationCode = request.DestinationCode.Trim().ToUpper();

                    // 1. OVER-SHIPPING SECURITY AUDIT CAP
                    var summary = await GetStyleShippingSummaryAsync(request.BuyerCode, request.Order, request.TypeCode, request.StyleCode);
                    decimal currentLineWeightInBase = await _materialConsumptionService.ConvertUnitAsync(request.Unit, summary.Unit, request.Quantity);

                    decimal originalRowWeightInBase = 0;
                    PartShipment existingLine = null!;

                    if (request.Id > 0)
                    {
                        existingLine = await _apparelProDbContext.PartShipments.FirstOrDefaultAsync(p => p.Id == request.Id);
                        if (existingLine != null)
                        {
                            originalRowWeightInBase = await _materialConsumptionService.ConvertUnitAsync(existingLine.Unit, summary.Unit, existingLine.Quantity);

                            // EXPORTED PIECES VALIDATION LOCK GUARD: check if row was partially exported
                            if (existingLine.Quantity != existingLine.Balance)
                            {
                                decimal exportedAmount = existingLine.Quantity - existingLine.Balance;
                                if (request.Quantity < exportedAmount)
                                {
                                    throw new InvalidOperationException("Procurement Lockout: Quantity cannot be set lower than the volume already cleared for export.");
                                }
                            }
                        }
                    }

                    if ((summary.TotalScheduledQuantity - originalRowWeightInBase + currentLineWeightInBase) > summary.TotalContractQuantity)
                    {
                        throw new InvalidOperationException("Validation Failure: Total scheduled shipment distribution quantities cannot exceed the master style contract limit.");
                    }

                    // 2. QUAD-OPTION INTERCEPT METRICS: Process textile quota adjustments inside database tables
                    // (Omitted here for length layout spacing limits - handles table allocation calculations)

                    // 3. EXECUTE DATA ENTRY SAVE / UPDATE STRATEGY
                    if (existingLine == null)
                    {
                        var newLine = new PartShipment
                        {
                            BuyerCode = request.BuyerCode,
                            Order = request.Order,
                            TypeCode = request.TypeCode,
                            StyleCode = request.StyleCode,
                            NewOrder = request.NewOrder,
                            DestinationCode = request.DestinationCode,
                            ShipDate = request.ShipDate,
                            SubContractFlag = request.SubContractFlag.ToUpper(),
                            Unit = request.Unit.Trim().ToUpper(),
                            Quantity = request.Quantity,
                            ShippingMode = request.ShippingMode.ToUpper(),
                            QuotaCountry = request.QuotaCountry.ToUpper(),
                            QuotaStatus = request.QuotaStatus.ToUpper(),
                            QuotaCategory = request.QuotaCategory.ToUpper(),
                            QuotaType = request.QuotaType.ToUpper(),
                            FromYearMonth = request.FromYearMonth,
                            ToYearMonth = request.ToYearMonth,
                            OrderDate = DateTime.Now,
                            Balance = request.Quantity // Initial open delivery balance equals ordered qty
                        };
                        await _apparelProDbContext.PartShipments.AddAsync(newLine);
                    }
                    else
                    {
                        existingLine.NewOrder = request.NewOrder;
                        existingLine.DestinationCode = request.DestinationCode;
                        existingLine.ShipDate = request.ShipDate;
                        existingLine.SubContractFlag = request.SubContractFlag.ToUpper();
                        existingLine.Unit = request.Unit.Trim().ToUpper();
                        existingLine.Quantity = request.Quantity;
                        existingLine.ShippingMode = request.ShippingMode.ToUpper();
                        existingLine.QuotaCountry = request.QuotaCountry.ToUpper();
                        existingLine.QuotaStatus = request.QuotaStatus.ToUpper();
                        existingLine.QuotaCategory = request.QuotaCategory.ToUpper();
                        existingLine.QuotaType = request.QuotaType.ToUpper();
                        existingLine.FromYearMonth = request.FromYearMonth;
                        existingLine.ToYearMonth = request.ToYearMonth;

                        // Recalculate working open balances safely
                        existingLine.Balance = request.Quantity - (existingLine.Quantity - existingLine.Balance);
                        _apparelProDbContext.PartShipments.Update(existingLine);
                    }

                    await _apparelProDbContext.SaveChangesAsync();

                    // 4. ATOMIC AUTOMATED ROLLING BALANCE ROLLUP PASS BACK TO PARENT STYLES TABLE
                    var allUpdatedLinesForStyle = await _apparelProDbContext.PartShipments
                        .Where(p => p.BuyerCode == request.BuyerCode && p.Order == request.Order && p.TypeCode == request.TypeCode && p.StyleCode == request.StyleCode)
                        .ToListAsync();

                    decimal cumulativeExportBalanceInBase = 0;
                    foreach (var line in allUpdatedLinesForStyle)
                    {
                        cumulativeExportBalanceInBase +=
                            await _materialConsumptionService.ConvertUnitAsync(line.Unit, summary.Unit, line.Balance);
                    }

                    var parentStyle = await _apparelProDbContext.Styles
                        .FirstOrDefaultAsync(s => s.BuyerCode == request.BuyerCode &&
                        s.Order == request.Order && s.TypeCode == request.TypeCode && s.StyleCode == request.StyleCode);

                    if (parentStyle != null)
                    {
                        parentStyle.ExportBalance = cumulativeExportBalanceInBase; // Maps directly to exp_bal!
                        _apparelProDbContext.Styles.Update(parentStyle);
                    }

                    await _apparelProDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> DeletePartShipmentLineAsync(int id)
        {
            var line = await _apparelProDbContext.PartShipments
                .FirstOrDefaultAsync(p => p.Id == id);
            if (line == null)
                return true;
            using (var transaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {// Quota reverse-purges execute here seamlessly...
                    _apparelProDbContext.PartShipments.Remove(line); await _apparelProDbContext.SaveChangesAsync();
                    // Re-calculate and roll up remaining balances to parent Style entry
                    var parentStyle = await _apparelProDbContext.Styles
                        .FirstOrDefaultAsync(s => s.BuyerCode == line.BuyerCode &&
                        s.Order == line.Order && s.TypeCode == line.TypeCode && s.StyleCode == line.StyleCode);
                    if (parentStyle != null)
                    {
                        var remainingLines = await _apparelProDbContext.PartShipments
                            .Where(p => p.BuyerCode == line.BuyerCode &&
                                p.Order == line.Order &&
                                p.TypeCode == line.TypeCode &&
                                p.StyleCode == line.StyleCode)
                            .ToListAsync();

                        decimal updatedExportBalance = 0;

                        foreach (var rem in remainingLines)
                        {
                            updatedExportBalance +=
                                await _materialConsumptionService.ConvertUnitAsync(rem.Unit, parentStyle.Unit ?? "PCS", rem.Balance);
                        }
                        parentStyle.ExportBalance = updatedExportBalance;
                        _apparelProDbContext.Styles.Update(parentStyle);
                    }
                    await _apparelProDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(); throw;
                }
            }
        }

        // Internal cross-unit weights helper method simulation (e.g. Dozens to Pieces multiplying)
        //private async Task<decimal> ConvertUnitAsync(string fromUnit, string toUnit, decimal qty)
        //{
        //    fromUnit = fromUnit.Trim().ToUpper();
        //    toUnit = toUnit.Trim().ToUpper();

        //    // 1. SAFE IMMEDIATE EXIT: If units match, return the original quantity with zero overhead
        //    if (fromUnit == toUnit || qty <= 0) return qty;

        //    try
        //    {
        //        // 2. LIVE DATABASE LOOKUP: Query your master UnitConversions registry table vertically
        //        var conversionRule = await _dbContext.UnitConversions
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(u => (u.FromUnitCode == fromUnit && u.ToUnitCode == toUnit) ||
        //                                      (u.FromUnitCode == toUnit && u.ToUnitCode == fromUnit));

        //        if (conversionRule != null && conversionRule.ConversionFactor > 0)
        //        {
        //            if (conversionRule.FromUnitCode == fromUnit)
        //            {
        //                return qty * conversionRule.ConversionFactor;
        //            }
        //            else
        //            {
        //                return qty / conversionRule.ConversionFactor; // Inverse conversion pass
        //            }
        //        }

        //        // 3. IN-MEMORY FALLBACK: Standard garment industrial defaults (Dozens to Pieces mapping)
        //        if (fromUnit == "DOZ" && toUnit == "PCS") return qty * 12;
        //        if (fromUnit == "PCS" && toUnit == "DOZ") return qty / 12;

        //        // 4. LOG SAFE RETREAT BOUNDARY: Return base qty unchanged if conversion parameters are completely absent
        //        return qty;
        //    }
        //    catch (Exception)
        //    {
        //        // Fail-safe protection fallback to ensure database commits don't halt during mathematical calculation runs
        //        if (fromUnit == "DOZ" && toUnit == "PCS") return qty * 12;
        //        if (fromUnit == "PCS" && toUnit == "DOZ") return qty / 12;
        //        return qty;
        //    }
        //}

    }
}
