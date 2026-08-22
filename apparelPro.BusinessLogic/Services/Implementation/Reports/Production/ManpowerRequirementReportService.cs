using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IManpowerRequirementReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_REP3.PRG's "MANPOWER REQUIREMENT" report - see
    // ManpowerRequirementReportServiceModel for the exact column semantics.
    public class ManpowerRequirementReportService : IManpowerRequirementReportService
    {
        private const string WorkHoursPerDayKey = "ProductionWorkHoursPerDay";
        private const string Efficiency1PercentKey = "ProductionEfficiency1Percent";
        private const string Efficiency2PercentKey = "ProductionEfficiency2Percent";
        private const string DefaultMachineCountPerLineKey = "ProductionDefaultMachineCountPerLine";

        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISystemParameterService _systemParameterService;

        public ManpowerRequirementReportService(
            ApparelProDbContext apparelProDbContext, ISystemParameterService systemParameterService)
        {
            _apparelProDbContext = apparelProDbContext;
            _systemParameterService = systemParameterService;
        }

        public async Task<ManpowerRequirementReportServiceModel> GetReportAsync(
            int buyerCode, string order, int typeCode, string styleCode, string? lineCode)
        {
            if (!string.IsNullOrEmpty(lineCode))
            {
                var lineExists = await _apparelProDbContext.ProductionLines
                    .AsNoTracking()
                    .AnyAsync(l => l.LineCode == lineCode);
                if (!lineExists)
                    throw new InvalidOperationException("Invalid Line Code.");
            }

            var breakdown = await _apparelProDbContext.StyleOperationBreakdowns
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order && d.TypeCode == typeCode && d.StyleCode == styleCode)
                .ToListAsync();

            // Mirrors legacy's "[ No Details for the Given Style ]" error box.
            if (breakdown.Count == 0)
                throw new InvalidOperationException("No Details for the Given Style.");

            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync() ?? buyerCode.ToString();

            var typeName = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => t.Id == typeCode)
                .Select(t => t.TypeName)
                .FirstOrDefaultAsync() ?? typeCode.ToString();

            var machineTypes = await _apparelProDbContext.MachineTypes
                .AsNoTracking()
                .OrderBy(m => m.Code)
                .ToListAsync();

            var sumByType = breakdown
                .GroupBy(d => d.MachineTypeCode)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Sam));

            var machineRows = new List<ManpowerMachineTimeRowServiceModel>();
            var manualRows = new List<ManpowerMachineTimeRowServiceModel>();
            decimal totalMachineTimeSam = 0;
            decimal grandTotalSam = 0;

            foreach (var machineType in machineTypes)
            {
                if (!sumByType.TryGetValue(machineType.Code, out var totalSam))
                    continue;

                var row = new ManpowerMachineTimeRowServiceModel
                {
                    MachineTypeCode = machineType.Code,
                    MachineTypeDescription = machineType.Description,
                    TotalSam = totalSam
                };

                grandTotalSam += totalSam;
                if (machineType.IsManual)
                {
                    manualRows.Add(row);
                }
                else
                {
                    machineRows.Add(row);
                    totalMachineTimeSam += totalSam;
                }
            }

            var workHoursPerDay = await _systemParameterService.GetDecimalValueAsync(WorkHoursPerDayKey, 8m);
            var eff1Percent = await _systemParameterService.GetDecimalValueAsync(Efficiency1PercentKey, 80m);
            var eff2Percent = await _systemParameterService.GetDecimalValueAsync(Efficiency2PercentKey, 65m);

            decimal machineCount;
            if (!string.IsNullOrEmpty(lineCode))
            {
                machineCount = await _apparelProDbContext.ProductionLineAllocations
                    .AsNoTracking()
                    .Where(a => a.LineCode == lineCode && a.BuyerCode == buyerCode && a.Order == order &&
                                a.TypeCode == typeCode && a.StyleCode == styleCode)
                    .SumAsync(a => (decimal?)a.NumberOfMachines) ?? 0;

                // Mirrors legacy's "Line : xx not allocated for given Style." error box.
                if (machineCount == 0)
                    throw new InvalidOperationException($"Line : {lineCode} not allocated for given Style.");
            }
            else
            {
                machineCount = await _systemParameterService.GetDecimalValueAsync(DefaultMachineCountPerLineKey, 50m);
            }

            var pcsPerMachineAt100 = totalMachineTimeSam == 0 ? 0 : workHoursPerDay * 60 / totalMachineTimeSam;
            var pcsPerMachineAtEff1 = pcsPerMachineAt100 * eff1Percent / 100;
            decimal? pcsPerMachineAtEff2 = eff2Percent == 0 ? null : pcsPerMachineAt100 * eff2Percent / 100;

            return new ManpowerRequirementReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyerName,
                Order = order,
                TypeCode = typeCode,
                TypeName = typeName,
                StyleCode = styleCode,
                LineCode = lineCode,
                MachineRows = machineRows,
                ManualRows = manualRows,
                TotalMachineTimeSam = totalMachineTimeSam,
                GrandTotalSam = grandTotalSam,
                PcsPerMachineAt100 = pcsPerMachineAt100,
                PcsPerMachineAtEff1 = pcsPerMachineAtEff1,
                PcsPerMachineAtEff2 = pcsPerMachineAtEff2,
                MachineCount = machineCount,
                TargetOutputAt100 = pcsPerMachineAt100 * machineCount,
                TargetOutputAtEff1 = pcsPerMachineAtEff1 * machineCount,
                TargetOutputAtEff2 = pcsPerMachineAtEff2 * machineCount,
                EstimatedStandardHours = grandTotalSam / 60,
                Eff1Percent = eff1Percent,
                Eff2Percent = eff2Percent,
                WorkHoursPerDay = workHoursPerDay
            };
        }
    }
}
