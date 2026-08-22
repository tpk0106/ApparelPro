using apparelPro.BusinessLogic.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionEntryService;
using apparelPro.BusinessLogic.SystemConfiguration;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    // SRP: owns Actual Production Entry persistence + the two cross-cutting
    // rules PR_DPRO2.PRG enforced (contract-section ceiling, line-schedule
    // slippage cascade). The cascade math itself lives entirely in
    // ProductionLineSlippageCalculator (DIP) - this class only resolves
    // which allocations are "downstream" from live data.
    public class DailyProductionEntryService : IDailyProductionEntryService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IHolidayService _holidayService;
        private readonly ISystemParameterLookupService _systemParameterLookupService;

        public DailyProductionEntryService(
            IMapper mapper, ApparelProDbContext apparelProDbContext, IHolidayService holidayService,
            ISystemParameterLookupService systemParameterLookupService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _holidayService = holidayService;
            _systemParameterLookupService = systemParameterLookupService;
        }

        public async Task<List<DailyProductionEntryServiceModel>> GetByDateAsync(
            DateOnly date, int buyerCode, string order, int typeCode, string styleCode, string lineCode)
        {
            var sections = await _apparelProDbContext.Sections
                .AsNoTracking()
                .OrderBy(s => s.Code)
                .ToListAsync();

            var todaysEntries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.Date == date && e.BuyerCode == buyerCode && e.Order == order &&
                            e.TypeCode == typeCode && e.StyleCode == styleCode && e.LineCode == lineCode)
                .ToListAsync();

            var toDateQuantityBySection = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.Date <= date && e.BuyerCode == buyerCode && e.Order == order &&
                            e.TypeCode == typeCode && e.StyleCode == styleCode && e.LineCode == lineCode)
                .GroupBy(e => e.SectionCode)
                .Select(g => new { SectionCode = g.Key, Total = g.Sum(e => e.Quantity) })
                .ToDictionaryAsync(g => g.SectionCode, g => g.Total);

            return sections.Select(section =>
            {
                var todaysEntry = todaysEntries.FirstOrDefault(e => e.SectionCode == section.Code);
                return new DailyProductionEntryServiceModel
                {
                    SectionCode = section.Code,
                    SectionDescription = section.Description,
                    Hours = todaysEntry?.Hours ?? 0,
                    Unit = todaysEntry?.Unit ?? string.Empty,
                    Quantity = todaysEntry?.Quantity ?? 0,
                    ToDateQuantity = toDateQuantityBySection.TryGetValue(section.Code, out var total) ? total : 0
                };
            }).ToList();
        }

        public async Task<List<DailyProductionEntryServiceModel>> BulkSaveAsync(
            DateOnly date, int buyerCode, string order, int typeCode, string styleCode, string lineCode,
            List<CreateDailyProductionEntryServiceModel> records)
        {
            var allocations = await _apparelProDbContext.ProductionLineAllocations
                .Where(a => a.BuyerCode == buyerCode && a.Order == order &&
                            a.TypeCode == typeCode && a.StyleCode == styleCode && a.LineCode == lineCode)
                .OrderBy(a => a.EstimatedStartDate)
                .ToListAsync();

            if (allocations.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Line '{lineCode}' is not allocated for this Buyer/Order/Type/Style.");
            }

            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var existingEntries = await _apparelProDbContext.DailyProductionEntries
                    .Where(e => e.Date == date && e.BuyerCode == buyerCode && e.Order == order &&
                                e.TypeCode == typeCode && e.StyleCode == styleCode && e.LineCode == lineCode)
                    .ToListAsync();

                if (existingEntries.Count > 0)
                {
                    _apparelProDbContext.DailyProductionEntries.RemoveRange(existingEntries);
                }

                var newEntities = records.Select(r => new DailyProductionEntry
                {
                    Date = date,
                    BuyerCode = buyerCode,
                    Order = order,
                    TypeCode = typeCode,
                    StyleCode = styleCode,
                    LineCode = lineCode,
                    SectionCode = r.SectionCode,
                    Hours = r.Hours,
                    Unit = r.Unit,
                    Quantity = r.Quantity
                }).ToList();

                if (newEntities.Count > 0)
                {
                    await _apparelProDbContext.DailyProductionEntries.AddRangeAsync(newEntities);
                }

                await ValidateContractSectionCeilingAsync(date, buyerCode, order, typeCode, styleCode, lineCode, records);

                // Legacy line_adj: if this entry's date has pushed the style's
                // own slot on this line past its currently planned end date,
                // the slot itself slips to the entry date and every other
                // allocation queued after it on the same physical line
                // dominoes later by the same amount.
                var currentSlot = allocations[^1];
                if (date > currentSlot.EstimatedEndDate)
                {
                    var holidays = await _holidayService.GetAllHolidayDatesAsync();
                    var downstream = await _apparelProDbContext.ProductionLineAllocations
                        .Where(a => a.LineCode == lineCode &&
                                    a.OriginalEstimatedStartDate > currentSlot.EstimatedEndDate)
                        .OrderBy(a => a.OriginalEstimatedStartDate)
                        .ToListAsync();

                    currentSlot.EstimatedEndDate = date;
                    currentSlot.IsCritical = true;

                    if (downstream.Count > 0)
                    {
                        var slots = downstream.Select(a => new LineAllocationScheduleSlot
                        {
                            AllocationKey = $"{a.BuyerCode}|{a.Order}|{a.TypeCode}|{a.StyleCode}|{a.ShipmentOrder}|{a.LineCode}",
                            NumberOfDays = a.NumberOfDays
                        }).ToList();

                        var slippageResults = ProductionLineSlippageCalculator.CascadeFrom(
                            date, slots, holidays.Contains);

                        foreach (var allocation in downstream)
                        {
                            var key = $"{allocation.BuyerCode}|{allocation.Order}|{allocation.TypeCode}|" +
                                      $"{allocation.StyleCode}|{allocation.ShipmentOrder}|{allocation.LineCode}";
                            var slip = slippageResults.First(r => r.AllocationKey == key);
                            allocation.EstimatedStartDate = slip.NewStartDate;
                            allocation.EstimatedEndDate = slip.NewEndDate;
                        }
                    }
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return await GetByDateAsync(date, buyerCode, order, typeCode, styleCode, lineCode);
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        private async Task ValidateContractSectionCeilingAsync(
            DateOnly date, int buyerCode, string order, int typeCode, string styleCode, string lineCode,
            List<CreateDailyProductionEntryServiceModel> records)
        {
            var contractSectionCode = await _systemParameterLookupService.GetValueAsync(
                SystemParameterKeys.ProductionContractSectionCode,
                SystemParameterKeys.ProductionContractSectionCodeDefault);

            // Prior days are unaffected by this save, so they can be read
            // straight from the database. Today's date, though, is exactly
            // what this save is about to replace (delete-then-reinsert) - at
            // this point that delete/insert is only tracked in-memory, not
            // yet flushed, so querying the database for Date == date would
            // still see the OLD persisted values, silently ignoring
            // whatever the user just edited. Use the incoming records
            // (the actual thing about to be saved) for today instead.
            var toDateQuantityBySection = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.Date < date && e.BuyerCode == buyerCode && e.Order == order &&
                            e.TypeCode == typeCode && e.StyleCode == styleCode && e.LineCode == lineCode)
                .GroupBy(e => e.SectionCode)
                .Select(g => new { SectionCode = g.Key, Total = g.Sum(e => e.Quantity) })
                .ToDictionaryAsync(g => g.SectionCode, g => g.Total);

            foreach (var record in records)
            {
                toDateQuantityBySection.TryGetValue(record.SectionCode, out var priorTotal);
                toDateQuantityBySection[record.SectionCode] = priorTotal + record.Quantity;
            }

            if (!toDateQuantityBySection.TryGetValue(contractSectionCode, out var contractToDateQuantity))
            {
                contractToDateQuantity = 0;
            }

            var violatingSection = toDateQuantityBySection
                .FirstOrDefault(kv => kv.Key != contractSectionCode && kv.Value > contractToDateQuantity);

            if (violatingSection.Key != null)
            {
                var sectionDescriptionByCode = await _apparelProDbContext.Sections
                    .AsNoTracking()
                    .Where(s => s.Code == violatingSection.Key || s.Code == contractSectionCode)
                    .ToDictionaryAsync(s => s.Code, s => s.Description);

                var violatingSectionName = sectionDescriptionByCode.GetValueOrDefault(violatingSection.Key, violatingSection.Key);
                var contractSectionName = sectionDescriptionByCode.GetValueOrDefault(contractSectionCode, contractSectionCode);

                throw new InvalidOperationException(
                    $"Section '{violatingSectionName}' to-date quantity ({violatingSection.Value}) cannot exceed the " +
                    $"contract section '{contractSectionName}' to-date quantity ({contractToDateQuantity}).");
            }
        }
    }
}
