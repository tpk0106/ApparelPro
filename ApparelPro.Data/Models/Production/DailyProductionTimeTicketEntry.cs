namespace ApparelPro.Data.Models.Production
{
    // PR_DPTT - one row per employee+operation entered against a production
    // line on a given date. The legacy screen's live efficiency-percentage
    // aggregate (per employee, across a ticket's rows) was never persisted
    // to disk - it lived in a session-only scratch table - so it isn't
    // modeled as an entity here either; it's computed on demand by
    // DailyProductionEfficiencyCalculator from these rows plus
    // StyleOperationBreakdowns.Sam.
    public class DailyProductionTimeTicketEntry
    {
        public DateOnly Date { get; set; }
        public string LineCode { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string OperationCode { get; set; } = null!;

        public decimal Quantity { get; set; }
        public string? NonProductiveHourCode { get; set; }
        public decimal NonProductiveHours { get; set; }
        public decimal WorkHours { get; set; }
    }
}
