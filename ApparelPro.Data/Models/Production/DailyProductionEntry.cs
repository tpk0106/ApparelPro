namespace ApparelPro.Data.Models.Production
{
    // PR_DPROD - actual daily output per section (Cutting, Sewing, etc.) for
    // a style on a production line. Stores only the day's entered quantity -
    // running/cumulative totals (legacy's to_dt_qty and the separate
    // PR_MPROD table) are computed on read via SUM, not stored, per the
    // Phase 5b design decision: a manually-cascaded running total is exactly
    // the class of denormalization bug a real database makes unnecessary.
    public class DailyProductionEntry
    {
        public DateOnly Date { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string LineCode { get; set; } = null!;
        public string SectionCode { get; set; } = null!;

        public decimal Hours { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
