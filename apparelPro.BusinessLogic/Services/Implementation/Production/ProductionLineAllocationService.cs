using apparelPro.BusinessLogic.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    // SRP: owns Production Line Allocation persistence + orchestrates the
    // scheduler; the two-pass allocation formula itself lives entirely in
    // ProductionLineSchedulingCalculator (DIP - a pure dependency this class
    // configures with live line/holiday data but never re-implements).
    public class ProductionLineAllocationService : IProductionLineAllocationService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IHolidayService _holidayService;
        private readonly IUnitConversionService _unitConversionService;

        public ProductionLineAllocationService(
            IMapper mapper, ApparelProDbContext apparelProDbContext,
            IHolidayService holidayService, IUnitConversionService unitConversionService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _holidayService = holidayService;
            _unitConversionService = unitConversionService;
        }

        public async Task<List<ProductionLineAllocationServiceModel>> GetByShipmentAsync(
            int buyerCode, string order, int typeCode, string styleCode, string shipmentOrder)
        {
            var entities = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .Where(a => a.BuyerCode == buyerCode && a.Order == order &&
                            a.TypeCode == typeCode && a.StyleCode == styleCode &&
                            a.ShipmentOrder == shipmentOrder)
                .ToListAsync();

            return _mapper.Map<List<ProductionLineAllocationServiceModel>>(entities);
        }

        public async Task<List<ProductionLineAllocationServiceModel>> GetByLineAsync(
            int buyerCode, string order, int typeCode, string styleCode, string lineCode)
        {
            var entities = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .Where(a => a.BuyerCode == buyerCode && a.Order == order &&
                            a.TypeCode == typeCode && a.StyleCode == styleCode && a.LineCode == lineCode)
                .OrderBy(a => a.EstimatedStartDate)
                .ToListAsync();

            return _mapper.Map<List<ProductionLineAllocationServiceModel>>(entities);
        }

        public async Task<ProductionLineAllocationServiceModel> ManualAllocateAsync(
            ManualAllocateProductionLineServiceModel model)
        {
            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var partShipment = await _apparelProDbContext.PartShipments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.BuyerCode == model.BuyerCode && p.Order == model.Order &&
                                               p.TypeCode == model.TypeCode && p.StyleCode == model.StyleCode &&
                                               p.NewOrder == model.ShipmentOrder)
                    ?? throw new KeyNotFoundException(
                        $"Shipment '{model.ShipmentOrder}' was not found for this style.");

                var line = await _apparelProDbContext.ProductionLines
                    .FirstOrDefaultAsync(l => l.LineCode == model.LineCode)
                    ?? throw new KeyNotFoundException($"Production line '{model.LineCode}' was not found.");

                var holidays = await _holidayService.GetAllHolidayDatesAsync();
                var days = ProductionLineSchedulingCalculator.ComputeDays(
                    model.TotalQuantity, model.EstimatedProductionPerDay, model.LeadTimeDays);
                var endDate = ProductionLineSchedulingCalculator.ExpandForHolidays(
                    model.EstimatedStartDate, days, holidays.Contains);

                var shipDate = DateOnly.FromDateTime(partShipment.ShipDate);

                // Manual allocation lets a planner pick any start date freely,
                // unlike automatic allocation which always queues off the
                // line's NextAllocationDate - so this is the one place an
                // overlap can actually be created. A physical line can only
                // run one job at a time, so two allocations (any style) can
                // never legitimately share a date range on the same line -
                // the Daily Production Entry slippage cascade also depends
                // on this never happening.
                var overlapping = await _apparelProDbContext.ProductionLineAllocations
                    .AsNoTracking()
                    .Where(a => a.LineCode == model.LineCode &&
                                !(a.BuyerCode == model.BuyerCode && a.Order == model.Order &&
                                  a.TypeCode == model.TypeCode && a.StyleCode == model.StyleCode &&
                                  a.ShipmentOrder == model.ShipmentOrder) &&
                                a.EstimatedStartDate <= endDate && a.EstimatedEndDate >= model.EstimatedStartDate)
                    .FirstOrDefaultAsync();

                if (overlapping != null)
                {
                    throw new InvalidOperationException(
                        $"Line '{model.LineCode}' is already committed to style '{overlapping.StyleCode}' " +
                        $"(shipment '{overlapping.ShipmentOrder}') from {overlapping.EstimatedStartDate:yyyy-MM-dd} " +
                        $"to {overlapping.EstimatedEndDate:yyyy-MM-dd}, which overlaps the requested " +
                        $"{model.EstimatedStartDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}. Choose a non-overlapping " +
                        $"date range or a different line.");
                }

                var entity = await _apparelProDbContext.ProductionLineAllocations
                    .FirstOrDefaultAsync(a => a.BuyerCode == model.BuyerCode && a.Order == model.Order &&
                                               a.TypeCode == model.TypeCode && a.StyleCode == model.StyleCode &&
                                               a.ShipmentOrder == model.ShipmentOrder && a.LineCode == model.LineCode);

                if (entity == null)
                {
                    entity = new ProductionLineAllocation
                    {
                        BuyerCode = model.BuyerCode,
                        Order = model.Order,
                        TypeCode = model.TypeCode,
                        StyleCode = model.StyleCode,
                        ShipmentOrder = model.ShipmentOrder,
                        LineCode = model.LineCode
                    };
                    _apparelProDbContext.ProductionLineAllocations.Add(entity);
                }

                entity.EstimatedProductionPerDay = model.EstimatedProductionPerDay;
                entity.TotalQuantity = model.TotalQuantity;
                entity.Unit = model.Unit;
                entity.LeadTimeDays = model.LeadTimeDays;
                entity.NumberOfMachines = model.NumberOfMachines;
                entity.CostPerDay = line.LineCostPerDay;
                entity.CurrencyCode = line.CurrencyCode;
                entity.NumberOfDays = days;
                // Legacy quirk preserved: PR_ESTM1.PRG's manual-save path sets
                // the "original" estimate columns to the current save's dates
                // every time, rather than only on first creation - so these
                // never actually diverge from EstimatedStartDate/EndDate here.
                entity.OriginalEstimatedStartDate = model.EstimatedStartDate;
                entity.OriginalEstimatedEndDate = endDate;
                entity.EstimatedStartDate = model.EstimatedStartDate;
                entity.EstimatedEndDate = endDate;
                entity.IsCritical = endDate >= shipDate;

                if (line.NextAllocationDate == null || line.NextAllocationDate < endDate)
                {
                    line.NextAllocationDate = endDate;
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return _mapper.Map<ProductionLineAllocationServiceModel>(entity);
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductionLineAllocationResultServiceModel> AutomaticAllocateAsync(
            AutomaticAllocateProductionLineServiceModel model)
        {
            var alreadyAllocated = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .AnyAsync(a => a.BuyerCode == model.BuyerCode && a.Order == model.Order &&
                               a.TypeCode == model.TypeCode && a.StyleCode == model.StyleCode &&
                               a.ShipmentOrder == model.ShipmentOrder);

            if (alreadyAllocated)
            {
                // Matches legacy exactly: automatic allocation is a one-time
                // action per shipment - if anything is already allocated
                // (by either mode), it silently does nothing further.
                var existing = await GetByShipmentAsync(
                    model.BuyerCode, model.Order, model.TypeCode, model.StyleCode, model.ShipmentOrder);
                return new ProductionLineAllocationResultServiceModel { Commits = existing, UnallocatedQuantity = 0 };
            }

            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var partShipment = await _apparelProDbContext.PartShipments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.BuyerCode == model.BuyerCode && p.Order == model.Order &&
                                               p.TypeCode == model.TypeCode && p.StyleCode == model.StyleCode &&
                                               p.NewOrder == model.ShipmentOrder)
                    ?? throw new KeyNotFoundException(
                        $"Shipment '{model.ShipmentOrder}' was not found for this style.");

                var shipDate = DateOnly.FromDateTime(partShipment.ShipDate);
                var quantity = await ConvertQuantityAsync(partShipment.Unit, model.Unit, partShipment.Quantity);

                var lines = await _apparelProDbContext.ProductionLines
                    .AsNoTracking()
                    .OrderBy(l => l.LineCode)
                    .ToListAsync();

                var holidays = await _holidayService.GetAllHolidayDatesAsync();

                // Pre-convert every line's MinimumProductionPerOrder into the
                // allocation's unit up front (one await per distinct
                // from-unit), so the candidate projection below can stay
                // synchronous instead of needing async-over-LINQ.
                var minimumQuantityByLine = new Dictionary<string, decimal>();
                foreach (var l in lines)
                {
                    minimumQuantityByLine[l.LineCode] =
                        await ConvertQuantityAsync(l.UnitCode, model.Unit, l.MinimumProductionPerOrder);
                }

                var candidates = lines.Select(l => new ProductionLineCandidate
                {
                    LineCode = l.LineCode,
                    NextAvailableDate = l.NextAllocationDate,
                    EstimatedProductionPerDay = model.EstimatedProductionPerDay,
                    MinimumProductionQuantity = minimumQuantityByLine[l.LineCode]
                }).ToList();

                var result = ProductionLineSchedulingCalculator.Allocate(candidates, new LineAllocationRequest
                {
                    TotalQuantity = quantity,
                    LeadTimeDays = model.LeadTimeDays,
                    ShipDate = shipDate,
                    IsHoliday = holidays.Contains
                });

                var persisted = new List<ProductionLineAllocationServiceModel>();
                foreach (var commit in result.Commits)
                {
                    var line = lines.First(l => l.LineCode == commit.LineCode);
                    var trackedLine = await _apparelProDbContext.ProductionLines
                        .FirstAsync(l => l.LineCode == commit.LineCode);

                    var entity = new ProductionLineAllocation
                    {
                        BuyerCode = model.BuyerCode,
                        Order = model.Order,
                        TypeCode = model.TypeCode,
                        StyleCode = model.StyleCode,
                        ShipmentOrder = model.ShipmentOrder,
                        LineCode = commit.LineCode,
                        EstimatedProductionPerDay = model.EstimatedProductionPerDay,
                        TotalQuantity = commit.Quantity,
                        Unit = model.Unit,
                        LeadTimeDays = model.LeadTimeDays,
                        NumberOfMachines = model.NumberOfMachines,
                        CostPerDay = line.LineCostPerDay,
                        CurrencyCode = line.CurrencyCode,
                        NumberOfDays = commit.NumberOfDays,
                        OriginalEstimatedStartDate = commit.StartDate,
                        OriginalEstimatedEndDate = commit.EndDate,
                        EstimatedStartDate = commit.StartDate,
                        EstimatedEndDate = commit.EndDate,
                        IsCritical = commit.IsCritical
                    };
                    _apparelProDbContext.ProductionLineAllocations.Add(entity);

                    if (trackedLine.NextAllocationDate == null || trackedLine.NextAllocationDate < commit.EndDate)
                    {
                        trackedLine.NextAllocationDate = commit.EndDate;
                    }

                    persisted.Add(_mapper.Map<ProductionLineAllocationServiceModel>(entity));
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new ProductionLineAllocationResultServiceModel
                {
                    Commits = persisted,
                    UnallocatedQuantity = result.UnallocatedQuantity
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(
            int buyerCode, string order, int typeCode, string styleCode, string shipmentOrder, string lineCode)
        {
            var entity = await _apparelProDbContext.ProductionLineAllocations
                .FirstOrDefaultAsync(a => a.BuyerCode == buyerCode && a.Order == order &&
                                           a.TypeCode == typeCode && a.StyleCode == styleCode &&
                                           a.ShipmentOrder == shipmentOrder && a.LineCode == lineCode);
            if (entity == null) return;

            // Legacy rolls the line's next-available date back to this
            // allocation's start date, but only if this was the most recent
            // commitment on that line (next_alc still equals this row's
            // end date) - otherwise a later allocation already moved it
            // forward and rolling back would understate the line's real load.
            var line = await _apparelProDbContext.ProductionLines
                .FirstOrDefaultAsync(l => l.LineCode == lineCode);
            if (line != null && line.NextAllocationDate == entity.EstimatedEndDate)
            {
                line.NextAllocationDate = entity.EstimatedStartDate;
            }

            _apparelProDbContext.ProductionLineAllocations.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }

        private async Task<decimal> ConvertQuantityAsync(string fromUnit, string toUnit, decimal quantity)
        {
            if (string.Equals(fromUnit, toUnit, StringComparison.OrdinalIgnoreCase)) return quantity;
            var conversion = await _unitConversionService.GetUnitConversionByFromUnitAndToUnitAsync(fromUnit, toUnit);
            return conversion?.Measure is decimal measure ? quantity * measure : quantity;
        }
    }
}
