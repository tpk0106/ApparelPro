namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionProgressGraphService
{
    // Replicates PR_PROG.PRG's "PRODUCTION PROGRESS" graph (Production
    // Control -> Production Progress Graph). Two cumulative series plotted
    // against a sequential elapsed-production-day index (1, 2, 3...), not
    // calendar date - so the estimated and actual trajectories can be
    // compared shape-for-shape even when they span different real dates.
    //
    // Estimated series: EstimatedProductionEntry rows for the style, summed
    // per date and made cumulative. This entity has no SectionCode at all
    // (it's a single planned-output-per-day figure representing final
    // output), so no section filtering applies or is needed.
    //
    // Actual series: DailyProductionEntry rows for the style, filtered to
    // the Final section only, same per-date summing and cumulative
    // treatment - matching the legacy's own "dele all for sect_cd # m_sect"
    // filter on the actual side.
    public class ProductionProgressPointServiceModel
    {
        public int DayNumber { get; set; }
        public decimal CumulativeQuantity { get; set; }
    }

    public class ProductionProgressGraphServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public List<ProductionProgressPointServiceModel> EstimatedSeries { get; set; } = new();
        public List<ProductionProgressPointServiceModel> ActualSeries { get; set; } = new();
    }
}
