using apparelPro.BusinessLogic.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionLineAllocationService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class EstimatedProductionLineAllocationService : IEstimatedProductionLineAllocationService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IHolidayService _holidayService;
        private readonly IUnitConversionService _unitConversionService;

        public EstimatedProductionLineAllocationService(
            IMapper mapper, ApparelProDbContext apparelProDbContext,
            IHolidayService holidayService, IUnitConversionService unitConversionService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _holidayService = holidayService;
            _unitConversionService = unitConversionService;
        }

        public async Task<EstimatedProductionLineAllocationServiceModel?> GetAsync(int buyerCode, string styleCode)
        {
            var entity = await _apparelProDbContext.EstimatedProductionLineAllocations
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.BuyerCode == buyerCode && a.StyleCode == styleCode);
            return entity == null ? null : _mapper.Map<EstimatedProductionLineAllocationServiceModel>(entity);
        }

        public async Task<EstimatedProductionLineAllocationServiceModel> ManualAllocateAsync(
            ManualAllocateEstimatedProductionLineServiceModel model)
        {
            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var line = await _apparelProDbContext.ProductionLines
                    .FirstOrDefaultAsync(l => l.LineCode == model.LineCode)
                    ?? throw new KeyNotFoundException($"Production line '{model.LineCode}' was not found.");

                var holidays = await _holidayService.GetAllHolidayDatesAsync();
                var days = ProductionLineSchedulingCalculator.ComputeDays(
                    model.TotalQuantity, model.EstimatedProductionPerDay, model.LeadTimeDays);
                var endDate = ProductionLineSchedulingCalculator.ExpandForHolidays(
                    model.EstimatedStartDate, days, holidays.Contains);

                var entity = await _apparelProDbContext.EstimatedProductionLineAllocations
                    .FirstOrDefaultAsync(a => a.BuyerCode == model.BuyerCode && a.StyleCode == model.StyleCode);

                if (entity == null)
                {
                    entity = new EstimatedProductionLineAllocation
                    {
                        BuyerCode = model.BuyerCode,
                        StyleCode = model.StyleCode
                    };
                    _apparelProDbContext.EstimatedProductionLineAllocations.Add(entity);
                }

                entity.EstimatedProductionPerDay = model.EstimatedProductionPerDay;
                entity.Unit = model.Unit;
                entity.LeadTimeDays = model.LeadTimeDays;
                entity.TotalQuantity = model.TotalQuantity;
                entity.ShipDate = model.ShipDate;
                entity.LineCode = model.LineCode;
                entity.NumberOfDays = days;
                entity.EstimatedStartDate = model.EstimatedStartDate;
                entity.EstimatedEndDate = endDate;
                entity.IsCritical = endDate >= model.ShipDate;

                if (line.EstimatedNextAllocationDate == null || line.EstimatedNextAllocationDate < endDate)
                {
                    line.EstimatedNextAllocationDate = endDate;
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return _mapper.Map<EstimatedProductionLineAllocationServiceModel>(entity);
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EstimatedProductionLineAllocationResultServiceModel> AutomaticAllocateAsync(
            AutomaticAllocateEstimatedProductionLineServiceModel model)
        {
            var alreadyAllocated = await _apparelProDbContext.EstimatedProductionLineAllocations
                .AsNoTracking()
                .AnyAsync(a => a.BuyerCode == model.BuyerCode && a.StyleCode == model.StyleCode);

            if (alreadyAllocated)
            {
                var existing = await GetAsync(model.BuyerCode, model.StyleCode);
                return new EstimatedProductionLineAllocationResultServiceModel
                {
                    Allocation = existing,
                    UnallocatedQuantity = 0
                };
            }

            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var lines = await _apparelProDbContext.ProductionLines
                    .AsNoTracking()
                    .OrderBy(l => l.LineCode)
                    .ToListAsync();

                var holidays = await _holidayService.GetAllHolidayDatesAsync();

                var minimumQuantityByLine = new Dictionary<string, decimal>();
                foreach (var l in lines)
                {
                    minimumQuantityByLine[l.LineCode] =
                        await ConvertQuantityAsync(l.UnitCode, model.Unit, l.MinimumProductionPerOrder);
                }

                var candidates = lines.Select(l => new ProductionLineCandidate
                {
                    LineCode = l.LineCode,
                    NextAvailableDate = l.EstimatedNextAllocationDate,
                    EstimatedProductionPerDay = model.EstimatedProductionPerDay,
                    MinimumProductionQuantity = minimumQuantityByLine[l.LineCode]
                }).ToList();

                var result = ProductionLineSchedulingCalculator.Allocate(candidates, new LineAllocationRequest
                {
                    TotalQuantity = model.TotalQuantity,
                    LeadTimeDays = model.LeadTimeDays,
                    ShipDate = model.ShipDate,
                    IsHoliday = holidays.Contains
                });

                EstimatedProductionLineAllocation? entity = null;

                // Legacy quirk (documented on the result model): the table's
                // key is Buyer+Style only, so each commit in this loop
                // overwrites the previous one - only the final commit's data
                // is what ends up persisted.
                foreach (var commit in result.Commits)
                {
                    var trackedLine = await _apparelProDbContext.ProductionLines
                        .FirstAsync(l => l.LineCode == commit.LineCode);

                    entity ??= await _apparelProDbContext.EstimatedProductionLineAllocations
                        .FirstOrDefaultAsync(a => a.BuyerCode == model.BuyerCode && a.StyleCode == model.StyleCode);

                    if (entity == null)
                    {
                        entity = new EstimatedProductionLineAllocation
                        {
                            BuyerCode = model.BuyerCode,
                            StyleCode = model.StyleCode
                        };
                        _apparelProDbContext.EstimatedProductionLineAllocations.Add(entity);
                    }

                    entity.EstimatedProductionPerDay = model.EstimatedProductionPerDay;
                    entity.Unit = model.Unit;
                    entity.LeadTimeDays = model.LeadTimeDays;
                    entity.TotalQuantity = commit.Quantity;
                    entity.ShipDate = model.ShipDate;
                    entity.LineCode = commit.LineCode;
                    entity.NumberOfDays = commit.NumberOfDays;
                    entity.EstimatedStartDate = commit.StartDate;
                    entity.EstimatedEndDate = commit.EndDate;
                    entity.IsCritical = commit.IsCritical;

                    if (trackedLine.EstimatedNextAllocationDate == null ||
                        trackedLine.EstimatedNextAllocationDate < commit.EndDate)
                    {
                        trackedLine.EstimatedNextAllocationDate = commit.EndDate;
                    }
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new EstimatedProductionLineAllocationResultServiceModel
                {
                    Allocation = entity == null ? null : _mapper.Map<EstimatedProductionLineAllocationServiceModel>(entity),
                    UnallocatedQuantity = result.UnallocatedQuantity
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int buyerCode, string styleCode)
        {
            var entity = await _apparelProDbContext.EstimatedProductionLineAllocations
                .FirstOrDefaultAsync(a => a.BuyerCode == buyerCode && a.StyleCode == styleCode);
            if (entity == null) return;

            var line = await _apparelProDbContext.ProductionLines
                .FirstOrDefaultAsync(l => l.LineCode == entity.LineCode);
            if (line != null && line.EstimatedNextAllocationDate == entity.EstimatedEndDate)
            {
                line.EstimatedNextAllocationDate = entity.EstimatedStartDate;
            }

            _apparelProDbContext.EstimatedProductionLineAllocations.Remove(entity);
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
