using apparelPro.BusinessLogic.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IStyleOperationBreakdownService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    // SRP: owns Style Operation Breakdown persistence + orchestrates the
    // line-balancing recalculation; the formula itself lives entirely in
    // OperationBreakdownBalanceCalculator (DIP - a pure, swappable dependency
    // with no DB/HTTP concerns of its own).
    public class StyleOperationBreakdownService : IStyleOperationBreakdownService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISystemParameterService _systemParameterService;

        private const string WorkHoursPerDayKey = "ProductionWorkHoursPerDay";
        private const string Efficiency2PercentKey = "ProductionEfficiency2Percent";
        private const string DefaultMachineCountPerLineKey = "ProductionDefaultMachineCountPerLine";

        public StyleOperationBreakdownService(
            IMapper mapper, ApparelProDbContext apparelProDbContext, ISystemParameterService systemParameterService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _systemParameterService = systemParameterService;
        }

        public async Task<List<StyleOperationBreakdownServiceModel>> GetBreakdownByStyleAsync(
            int buyerCode, string order, int typeCode, string styleCode)
        {
            var entities = await _apparelProDbContext.StyleOperationBreakdowns
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order &&
                            d.TypeCode == typeCode && d.StyleCode == styleCode)
                .OrderBy(d => d.ComponentSequence).ThenBy(d => d.OperationNumber)
                .ToListAsync();

            return _mapper.Map<List<StyleOperationBreakdownServiceModel>>(entities);
        }

        public async Task<List<StyleOperationBreakdownServiceModel>> SeedFromTemplateAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            int componentSequence, string componentCode)
        {
            // Legacy f_flg: the auto-copy only ever runs when the WHOLE STYLE has
            // zero operation-breakdown rows yet, not just this one component -
            // PR_OPD2.PRG checks this once before the per-component loop begins.
            var styleAlreadyHasOperations = await _apparelProDbContext.StyleOperationBreakdowns
                .AsNoTracking()
                .AnyAsync(d => d.BuyerCode == buyerCode && d.Order == order &&
                               d.TypeCode == typeCode && d.StyleCode == styleCode);

            if (styleAlreadyHasOperations)
            {
                return await GetBreakdownByStyleAsync(buyerCode, order, typeCode, styleCode);
            }

            var templates = await _apparelProDbContext.ComponentOperationTemplates
                .AsNoTracking()
                .Where(t => t.ComponentCode == componentCode)
                .OrderBy(t => t.OperationSequence)
                .ToListAsync();

            var workHoursPerDay = await _systemParameterService.GetDecimalValueAsync(WorkHoursPerDayKey, 8m);

            var seeded = templates.Select(t => new StyleOperationBreakdown
            {
                BuyerCode = buyerCode,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                ComponentSequence = componentSequence,
                OperationNumber = t.OperationSequence,
                ComponentCode = componentCode,
                OperationCode = t.OperationCode,
                MachineTypeCode = t.MachineTypeCode,
                Sam = t.Sam,
                Quota = t.Sam == 0 ? 0 : (workHoursPerDay * 60m) / t.Sam,
                NumberOfMachines = t.NumberOfMachines
            }).ToList();

            if (seeded.Count > 0)
            {
                await _apparelProDbContext.StyleOperationBreakdowns.AddRangeAsync(seeded);
                await _apparelProDbContext.SaveChangesAsync();
            }

            return _mapper.Map<List<StyleOperationBreakdownServiceModel>>(seeded);
        }

        public async Task<StyleOperationBreakdownSaveResultServiceModel> BulkSaveAndRecalculateAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            List<CreateStyleOperationBreakdownServiceModel> records)
        {
            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var existingRecords = await _apparelProDbContext.StyleOperationBreakdowns
                    .Where(d => d.BuyerCode == buyerCode && d.Order == order &&
                                d.TypeCode == typeCode && d.StyleCode == styleCode)
                    .ToListAsync();

                if (existingRecords.Count > 0)
                {
                    _apparelProDbContext.StyleOperationBreakdowns.RemoveRange(existingRecords);
                }

                var workHoursPerDay = await _systemParameterService.GetDecimalValueAsync(WorkHoursPerDayKey, 8m);
                var efficiency2Percent = await _systemParameterService.GetDecimalValueAsync(Efficiency2PercentKey, 65m);
                var defaultMachineCountPerLine = await _systemParameterService.GetDecimalValueAsync(DefaultMachineCountPerLineKey, 50m);

                var machineTypeCodes = records.Select(r => r.MachineTypeCode).Distinct().ToList();
                var manualFlagsByCode = await _apparelProDbContext.MachineTypes
                    .AsNoTracking()
                    .Where(m => machineTypeCodes.Contains(m.Code))
                    .ToDictionaryAsync(m => m.Code, m => m.IsManual);

                var newEntities = records.Select(r => new StyleOperationBreakdown
                {
                    BuyerCode = buyerCode,
                    Order = order,
                    TypeCode = typeCode,
                    StyleCode = styleCode,
                    ComponentSequence = r.ComponentSequence,
                    OperationNumber = r.OperationNumber,
                    ComponentCode = r.ComponentCode,
                    OperationCode = r.OperationCode,
                    MachineTypeCode = r.MachineTypeCode,
                    Sam = r.Sam
                }).ToList();

                // Row identity for matching calculator results back to entities:
                // OperationNumber is unique per style (part of the composite key),
                // so it's a safe correlation key without needing entity references.
                var calculatorInputs = newEntities.Select(e => new OperationSamInput
                {
                    OperationKey = e.OperationNumber.ToString(),
                    Sam = e.Sam,
                    IsManualMachineType = manualFlagsByCode.TryGetValue(e.MachineTypeCode, out var isManual) && isManual
                }).ToList();

                var balanceResult = OperationBreakdownBalanceCalculator.Calculate(
                    calculatorInputs, workHoursPerDay, efficiency2Percent, defaultMachineCountPerLine);

                var resultsByKey = balanceResult.Operations.ToDictionary(o => o.OperationKey);
                foreach (var entity in newEntities)
                {
                    var result = resultsByKey[entity.OperationNumber.ToString()];
                    entity.Quota = result.Quota;
                    entity.NumberOfMachines = result.MachinesNeeded;
                }

                if (newEntities.Count > 0)
                {
                    await _apparelProDbContext.StyleOperationBreakdowns.AddRangeAsync(newEntities);
                }

                var capacity = await _apparelProDbContext.StyleProductionCapacities
                    .FirstOrDefaultAsync(c => c.BuyerCode == buyerCode && c.Order == order &&
                                               c.TypeCode == typeCode && c.StyleCode == styleCode);
                if (capacity == null)
                {
                    capacity = new StyleProductionCapacity
                    {
                        BuyerCode = buyerCode,
                        Order = order,
                        TypeCode = typeCode,
                        StyleCode = styleCode
                    };
                    _apparelProDbContext.StyleProductionCapacities.Add(capacity);
                }
                capacity.OutputPerDay = balanceResult.TargetDailyOutput;

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new StyleOperationBreakdownSaveResultServiceModel
                {
                    TargetDailyOutput = balanceResult.TargetDailyOutput,
                    Operations = _mapper.Map<List<StyleOperationBreakdownServiceModel>>(newEntities)
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
    }
}
