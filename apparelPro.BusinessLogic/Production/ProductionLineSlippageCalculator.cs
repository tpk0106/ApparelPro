namespace apparelPro.BusinessLogic.Production
{
    public class LineAllocationScheduleSlot
    {
        public required string AllocationKey { get; init; }
        public required decimal NumberOfDays { get; init; }
    }

    public class LineSlippageResult
    {
        public required string AllocationKey { get; init; }
        public required DateOnly NewStartDate { get; init; }
        public required DateOnly NewEndDate { get; init; }
    }

    // Ported from PR_DPRO2.PRG's line_adj procedure: when actual production
    // runs past a line allocation's estimated end date, every other
    // allocation queued after it on the same physical line dominoes later
    // by the same amount. Pure/DB-free like the other calculators - the
    // service layer resolves which allocations are "downstream" and in
    // what order; this class only computes the new dates.
    public static class ProductionLineSlippageCalculator
    {
        public static IReadOnlyList<LineSlippageResult> CascadeFrom(
            DateOnly triggeringNewEndDate,
            IReadOnlyList<LineAllocationScheduleSlot> downstreamSlotsInOriginalStartDateOrder,
            Func<DateOnly, bool> isHoliday)
        {
            var results = new List<LineSlippageResult>();
            var nextStart = NextWorkingDay(triggeringNewEndDate.AddDays(1), isHoliday);

            foreach (var slot in downstreamSlotsInOriginalStartDateOrder)
            {
                // Legacy: m_e_dt = m_s_dt + if(no_days=1,1,no_days-1), then
                // expanded forward over any holidays in the span.
                var span = slot.NumberOfDays <= 1 ? 1 : slot.NumberOfDays - 1;
                var endDate = ProductionLineSchedulingCalculator.ExpandForHolidays(nextStart, span, isHoliday);

                results.Add(new LineSlippageResult
                {
                    AllocationKey = slot.AllocationKey,
                    NewStartDate = nextStart,
                    NewEndDate = endDate
                });

                nextStart = NextWorkingDay(endDate.AddDays(1), isHoliday);
            }

            return results;
        }

        private static DateOnly NextWorkingDay(DateOnly date, Func<DateOnly, bool> isHoliday)
        {
            while (isHoliday(date))
            {
                date = date.AddDays(1);
            }
            return date;
        }
    }
}
