namespace apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionScheduleReportService
{
    // Replicates PR_ESTL2.PRG's "ESTIMATED PRODUCTION SCHEDULE" report
    // (Reports -> Estimated Production Schedule). Flat listing of
    // EstimatedProductionLineAllocation rows (PR_ESTLN - pre-order planning
    // data, keyed by Buyer+Style only) within the given date range,
    // sorted Line -> EstStartDate -> Buyer -> Style. Float = ShipDate -
    // EstimatedEndDate, same concept as Report A (Production Schedule).
    public class EstimatedProductionScheduleRowServiceModel
    {
        public string LineCode { get; set; } = null!;
        public DateOnly EstStartDate { get; set; }
        public DateOnly EstEndDate { get; set; }
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public decimal EstimatedProductionPerDay { get; set; }
        public string Unit { get; set; } = null!;
        public decimal LeadTimeDays { get; set; }
        public decimal NumberOfDays { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateOnly ShipDate { get; set; }
        public int FloatDays { get; set; }
    }

    public class EstimatedProductionScheduleReportServiceModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public List<EstimatedProductionScheduleRowServiceModel> Rows { get; set; } = new();
    }
}
