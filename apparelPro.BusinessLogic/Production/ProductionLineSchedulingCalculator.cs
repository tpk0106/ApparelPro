namespace apparelPro.BusinessLogic.Production
{
    public class ProductionLineCandidate
    {
        public required string LineCode { get; init; }
        // Null means the line is idle - the legacy screen back-calculates a
        // start date from the ship date in that case (PR_ESTM1.PRG line 475 /
        // PR_ESTL1.PRG line 337: iif(!empty(next_alc), next_alc, ship_date-days)).
        public DateOnly? NextAvailableDate { get; init; }
        public required decimal EstimatedProductionPerDay { get; init; }
        public required decimal MinimumProductionQuantity { get; init; }
    }

    public class LineAllocationRequest
    {
        public required decimal TotalQuantity { get; init; }
        public required decimal LeadTimeDays { get; init; }
        public required DateOnly ShipDate { get; init; }
        public required Func<DateOnly, bool> IsHoliday { get; init; }
    }

    public class LineAllocationCommit
    {
        public required string LineCode { get; init; }
        public required decimal Quantity { get; init; }
        public required decimal NumberOfDays { get; init; }
        public required DateOnly StartDate { get; init; }
        public required DateOnly EndDate { get; init; }
        public required bool IsCritical { get; init; }
    }

    public class LineAllocationResult
    {
        public required IReadOnlyList<LineAllocationCommit> Commits { get; init; }
        // >0 means the scheduler could not place the full quantity - the
        // legacy screens show "Unable to allocate Production Lines" in this case.
        public required decimal UnallocatedQuantity { get; init; }
    }

    // Ported from PR_ESTM1.PRG (lines 457-583) / PR_ESTL1.PRG (lines 329-432) -
    // both screens run the identical two-pass algorithm against different
    // tables, so it's extracted once here (SRP/DRY) rather than duplicated.
    // Pure and DB-free like OperationBreakdownBalanceCalculator: the calendar
    // is injected as a predicate so this class never touches EF Core.
    public static class ProductionLineSchedulingCalculator
    {
        public static LineAllocationResult Allocate(
            IReadOnlyList<ProductionLineCandidate> lines,
            LineAllocationRequest request)
        {
            // Pass 1: does any single line, taken in order, finish the whole
            // quantity before the ship date?
            foreach (var line in lines)
            {
                var days = ComputeDays(request.TotalQuantity, line.EstimatedProductionPerDay, request.LeadTimeDays);
                var startDate = line.NextAvailableDate ?? request.ShipDate.AddDays(-(int)Math.Ceiling(days));
                var endDate = ExpandForHolidays(startDate, days, request.IsHoliday);

                if (endDate <= request.ShipDate)
                {
                    var commit = new LineAllocationCommit
                    {
                        LineCode = line.LineCode,
                        Quantity = request.TotalQuantity,
                        NumberOfDays = days,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsCritical = endDate >= request.ShipDate
                    };
                    return new LineAllocationResult
                    {
                        Commits = new[] { commit },
                        UnallocatedQuantity = 0
                    };
                }
            }

            // Pass 2: no single line could take it all - split across lines by
            // whatever capacity each has available before the ship date.
            // Legacy quirk preserved deliberately: a line's full available
            // capacity is committed even if that overshoots what's still
            // needed (PR_ESTM1.PRG never caps m_tot_qty to the remaining
            // amount) - the final line in the chain can allocate slightly
            // more than required rather than exactly the remainder.
            var remaining = request.TotalQuantity;
            var commits2 = new List<LineAllocationCommit>();

            foreach (var line in lines)
            {
                if (remaining <= 0) break;
                if (line.NextAvailableDate == null) continue;

                var startDate = line.NextAvailableDate.Value;
                var availableDays = CountWorkingDays(startDate, request.ShipDate, request.IsHoliday);
                var capacity = availableDays * line.EstimatedProductionPerDay;

                if (capacity >= line.MinimumProductionQuantity)
                {
                    commits2.Add(new LineAllocationCommit
                    {
                        LineCode = line.LineCode,
                        Quantity = capacity,
                        NumberOfDays = availableDays,
                        StartDate = startDate,
                        EndDate = request.ShipDate,
                        // Pass-2 commits always end exactly on the ship date
                        // (legacy sets m_e_dt = m_shp_dt directly), and the
                        // critical rule is "not critical only if EndDate <
                        // ShipDate" - so every pass-2 commit is critical.
                        IsCritical = true
                    });
                    remaining -= capacity;
                }
            }

            return new LineAllocationResult
            {
                Commits = commits2,
                UnallocatedQuantity = Math.Max(remaining, 0)
            };
        }

        // Exposed publicly: the Manual allocation path (user supplies the
        // line and start date directly, skipping the two-pass search) still
        // needs the identical day-count/holiday-expansion math the automatic
        // path uses, so the service layer calls these instead of duplicating
        // the formula.
        public static decimal ComputeDays(decimal quantity, decimal estimatedProductionPerDay, decimal leadTimeDays)
        {
            if (estimatedProductionPerDay == 0) return leadTimeDays;
            var rawDays = Math.Round(quantity / estimatedProductionPerDay, 1, MidpointRounding.AwayFromZero);
            return rawDays + leadTimeDays;
        }

        public static DateOnly ExpandForHolidays(DateOnly startDate, decimal days, Func<DateOnly, bool> isHoliday)
        {
            var endDate = startDate.AddDays((int)Math.Ceiling(days));
            var current = startDate;
            while (current <= endDate)
            {
                if (isHoliday(current))
                {
                    endDate = endDate.AddDays(1);
                }
                current = current.AddDays(1);
            }
            return endDate;
        }

        private static decimal CountWorkingDays(DateOnly startDate, DateOnly endDate, Func<DateOnly, bool> isHoliday)
        {
            decimal count = 0;
            var current = startDate;
            while (current <= endDate)
            {
                if (!isHoliday(current)) count++;
                current = current.AddDays(1);
            }
            return count;
        }
    }
}
