namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryDailyReportService
{
    // Replicates PR_DPROD.PRG's "DAILY PRODUCTION SUMMARY" report (Reports ->
    // Production Summary (Daily)). One row per Buyer/Order/Style/Line active
    // on the given date, with one Pro Qty/To-Date Qty/Balance column-group
    // per Section (dynamic, from the Sections master - same pattern as
    // Actual Production Entry).
    public class ProductionSummaryDailySectionTotalServiceModel
    {
        public string SectionCode { get; set; } = null!;
        public decimal ProQuantity { get; set; }
        public decimal ToDateQuantity { get; set; }
        public decimal Balance { get; set; }
    }

    public class ProductionSummaryDailyLineServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;

        // From PurchaseOrders.Description - the legacy od_po.desc free-text
        // order description.
        public string? Description { get; set; }

        // Style's own master unit (Styles.Unit) - every quantity on this row
        // is converted into it, same as the legacy report's CONVERT() calls.
        public string Unit { get; set; } = null!;

        public string LineCode { get; set; } = null!;

        // Sum of every ProductionLineAllocation committed to this line for
        // this style, converted to Unit. Legacy took only the first matching
        // allocation (a single seek) - a quirk of how PR_LNAL was indexed,
        // not a deliberate one-allocation-per-line rule, since a style can
        // legitimately have multiple shipments queued on the same line.
        // Summing all of them is the more correct modern equivalent.
        public decimal OrderQuantity { get; set; }

        public List<ProductionSummaryDailySectionTotalServiceModel> Sections { get; set; } = new();
    }

    public class ProductionSummaryDailyReportServiceModel
    {
        public DateOnly Date { get; set; }
        public List<string> SectionCodes { get; set; } = new();
        public List<string> SectionDescriptions { get; set; } = new();
        public List<ProductionSummaryDailyLineServiceModel> Lines { get; set; } = new();

        public decimal TotalOrderQuantity { get; set; }

        // Per-section grand totals. Balance here is TotalOrderQuantity minus
        // that section's total ToDateQuantity across every line - NOT a sum
        // of the individual line balances. That's a genuine legacy quirk
        // (PR_DPROD.PRG computes the totals row exactly this way, with the
        // per-line balance sum commented out in the source) preserved here
        // rather than "corrected", since it's what the report actually
        // printed.
        public List<ProductionSummaryDailySectionTotalServiceModel> Totals { get; set; } = new();
    }
}
