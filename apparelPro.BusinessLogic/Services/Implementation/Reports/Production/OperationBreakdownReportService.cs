using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IOperationBreakdownReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_REP1.PRG's "OPERATION BREAKDOWN" report - see
    // OperationBreakdownReportServiceModel for the exact column semantics
    // and why the legacy output-projection block is intentionally omitted.
    public class OperationBreakdownReportService : IOperationBreakdownReportService
    {
        private const string WorkHoursPerDayKey = "ProductionWorkHoursPerDay";
        private const string Efficiency1PercentKey = "ProductionEfficiency1Percent";
        private const string Efficiency2PercentKey = "ProductionEfficiency2Percent";

        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISystemParameterService _systemParameterService;

        public OperationBreakdownReportService(
            ApparelProDbContext apparelProDbContext, ISystemParameterService systemParameterService)
        {
            _apparelProDbContext = apparelProDbContext;
            _systemParameterService = systemParameterService;
        }

        public async Task<OperationBreakdownReportServiceModel> GetReportAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            var breakdown = await _apparelProDbContext.StyleOperationBreakdowns
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order && d.TypeCode == typeCode && d.StyleCode == styleCode)
                .OrderBy(d => d.ComponentSequence).ThenBy(d => d.OperationNumber)
                .ToListAsync();

            // Mirrors legacy's "Operation breakdown for given style not entered" error box.
            if (breakdown.Count == 0)
                throw new InvalidOperationException("Operation breakdown for given style not entered.");

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

            var operationCodes = breakdown.Select(d => d.OperationCode).Distinct().ToList();
            var operationDescriptions = await _apparelProDbContext.Operations
                .AsNoTracking()
                .Where(o => operationCodes.Contains(o.OperationCode))
                .ToDictionaryAsync(o => o.OperationCode, o => o.Description);

            var componentCodes = breakdown.Select(d => d.ComponentCode).Distinct().ToList();
            var componentDescriptions = await _apparelProDbContext.GarmentComponents
                .AsNoTracking()
                .Where(c => componentCodes.Contains(c.ComponentCode))
                .ToDictionaryAsync(c => c.ComponentCode, c => c.Description);

            var workHoursPerDay = await _systemParameterService.GetDecimalValueAsync(WorkHoursPerDayKey, 8m);
            var eff1Percent = await _systemParameterService.GetDecimalValueAsync(Efficiency1PercentKey, 80m);
            var eff2Percent = await _systemParameterService.GetDecimalValueAsync(Efficiency2PercentKey, 65m);

            var groups = new List<OperationBreakdownComponentGroupServiceModel>();
            var displayOperationNo = 0;

            foreach (var componentGroup in breakdown.GroupBy(d => new { d.ComponentSequence, d.ComponentCode }).OrderBy(g => g.Key.ComponentSequence))
            {
                var rows = new List<OperationBreakdownRowServiceModel>();
                foreach (var detail in componentGroup)
                {
                    displayOperationNo++;

                    var quotaAtEff1 = detail.Quota * eff1Percent / 100;
                    var quotaAtEff2 = detail.Quota * eff2Percent / 100;

                    rows.Add(new OperationBreakdownRowServiceModel
                    {
                        DisplayOperationNo = displayOperationNo,
                        OperationCode = detail.OperationCode,
                        OperationDescription = operationDescriptions.GetValueOrDefault(detail.OperationCode, ""),
                        MachineTypeCode = detail.MachineTypeCode,
                        Sam = detail.Sam,
                        QuotaAt100 = detail.Quota,
                        QuotaAtEff1 = quotaAtEff1,
                        QuotaPcsPer2HrsAtEff1 = workHoursPerDay == 0 ? 0 : quotaAtEff1 / workHoursPerDay * 2,
                        QuotaAtEff2 = quotaAtEff2,
                        QuotaPcsPer2HrsAtEff2 = workHoursPerDay == 0 ? 0 : quotaAtEff2 / workHoursPerDay * 2,
                        NumberOfMachines = detail.NumberOfMachines,
                        NumberOfOperators = Math.Round(detail.NumberOfMachines, 0, MidpointRounding.AwayFromZero)
                    });
                }

                groups.Add(new OperationBreakdownComponentGroupServiceModel
                {
                    ComponentCode = componentGroup.Key.ComponentCode,
                    ComponentDescription = componentDescriptions.GetValueOrDefault(componentGroup.Key.ComponentCode, ""),
                    Rows = rows
                });
            }

            return new OperationBreakdownReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyerName,
                Order = order,
                TypeCode = typeCode,
                TypeName = typeName,
                StyleCode = styleCode,
                Eff1Percent = eff1Percent,
                Eff2Percent = eff2Percent,
                WorkHoursPerDay = workHoursPerDay,
                Groups = groups
            };
        }
    }
}
