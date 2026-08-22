namespace apparelPro.BusinessLogic.Production
{
    public class ProductionEntryInput
    {
        public required string EmployeeCode { get; init; }
        public required decimal Quantity { get; init; }
        public required decimal Sam { get; init; }
        public required decimal NonProductiveHours { get; init; }
        public required decimal WorkHours { get; init; }
    }

    public class EmployeeEfficiencySummary
    {
        public required string EmployeeCode { get; init; }
        public required decimal WorkHours { get; init; }
        public required decimal NonProductiveHours { get; init; }
        public required decimal EarnedMinutes { get; init; }
        public required decimal OverEfficiencyPercent { get; init; }
        public required decimal OperatorEfficiencyPercent { get; init; }
    }

    // Ported from PR_DPTT1.PRG's live per-employee aggregate (never
    // persisted in legacy - a session-only scratch table, rebuilt from the
    // detail rows every time the screen opens). Pure/DB-free, same pattern
    // as the Phase 2/3 calculators: computed fresh from ticket rows + each
    // operation's SAM (sourced from StyleOperationBreakdowns by the service
    // layer, not this class).
    public static class DailyProductionEfficiencyCalculator
    {
        public static IReadOnlyList<EmployeeEfficiencySummary> Summarize(IReadOnlyList<ProductionEntryInput> entries)
        {
            return entries
                .GroupBy(e => e.EmployeeCode)
                .Select(g =>
                {
                    var earnedMinutes = g.Sum(e => e.Quantity * e.Sam);
                    var nonProductiveHours = g.Sum(e => e.NonProductiveHours);
                    // Legacy quirk preserved: WorkHours is stored per detail
                    // row, not per employee - taking the first row's value
                    // here (all of an employee's rows in one ticket are
                    // expected to carry the same work-hours-for-the-day).
                    var workHours = g.First().WorkHours;

                    var overEfficiencyPercent = workHours == 0
                        ? 0m
                        : (earnedMinutes / 60m) / workHours * 100m;

                    var availableHours = workHours - nonProductiveHours;
                    var operatorEfficiencyPercent = availableHours == 0
                        ? 0m
                        : (earnedMinutes / 60m) / availableHours * 100m;

                    return new EmployeeEfficiencySummary
                    {
                        EmployeeCode = g.Key,
                        WorkHours = workHours,
                        NonProductiveHours = nonProductiveHours,
                        EarnedMinutes = earnedMinutes,
                        OverEfficiencyPercent = overEfficiencyPercent,
                        OperatorEfficiencyPercent = operatorEfficiencyPercent
                    };
                })
                .ToList();
        }
    }
}
