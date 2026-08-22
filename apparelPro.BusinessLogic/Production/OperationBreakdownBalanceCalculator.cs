namespace apparelPro.BusinessLogic.Production
{
    public class OperationSamInput
    {
        public required string OperationKey { get; init; }
        public required decimal Sam { get; init; }
        public required bool IsManualMachineType { get; init; }
    }

    public class OperationBalanceResult
    {
        public required string OperationKey { get; init; }
        public required decimal Quota { get; init; }
        public required decimal MachinesNeeded { get; init; }
    }

    public class LineBalanceResult
    {
        public required decimal TargetDailyOutput { get; init; }
        public required IReadOnlyList<OperationBalanceResult> Operations { get; init; }
    }

    // Ported from PR_OPD2.PRG's "Saving Entries" routine (lines 229-283).
    // SRP: isolates the line-balancing formula from persistence/orchestration
    // (StyleOperationBreakdownService) so it can be exercised with plain
    // numbers in a unit test, no DbContext required. DIP: the service depends
    // on this directly since it has no external dependencies of its own to
    // swap out - there's nothing an interface would add here.
    public static class OperationBreakdownBalanceCalculator
    {
        public static LineBalanceResult Calculate(
            IReadOnlyList<OperationSamInput> operations,
            decimal workHoursPerDay,
            decimal efficiency2Percent,
            decimal defaultMachineCountPerLine)
        {
            var totalMachineSam = operations
                .Where(o => !o.IsManualMachineType)
                .Sum(o => o.Sam);

            // Mirrors Clipper's zdiv() - a style with zero machine-bound SAM
            // (fully manual garment) is a valid case, not an error condition.
            var piecesPerMachinePerDay = totalMachineSam == 0
                ? 0m
                : (workHoursPerDay * 60m) / totalMachineSam;

            var adjustedPiecesPerMachine = piecesPerMachinePerDay * (efficiency2Percent / 100m);
            var targetDailyOutput = adjustedPiecesPerMachine * defaultMachineCountPerLine;

            var results = operations.Select(o =>
            {
                var quota = o.Sam == 0 ? 0m : (workHoursPerDay * 60m) / o.Sam;
                var denominator = quota * (efficiency2Percent / 100m);
                var machinesNeeded = denominator == 0 ? 0m : targetDailyOutput / denominator;

                return new OperationBalanceResult
                {
                    OperationKey = o.OperationKey,
                    Quota = quota,
                    MachinesNeeded = machinesNeeded
                };
            }).ToList();

            return new LineBalanceResult
            {
                TargetDailyOutput = targetDailyOutput,
                Operations = results
            };
        }
    }
}
