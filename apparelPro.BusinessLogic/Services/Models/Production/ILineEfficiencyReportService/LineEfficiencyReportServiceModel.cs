namespace apparelPro.BusinessLogic.Services.Models.Production.ILineEfficiencyReportService
{
    // Replicates PR_GRH1.PRG's "LINE EFFICIENCY" report (Reports ->
    // Production Line Efficiency). Only Final-section DailyProductionEntries
    // for the given Line/month are considered. Per user confirmation, the
    // legacy loop-overwrite quirk (a multi-style day valued using only the
    // LAST style's std-hours-per-unit/machine-count) is intentionally NOT
    // preserved here - this computes an earned-hours-weighted percentage
    // instead:
    //   EarnedHours(day)  = sum over styles produced that day of
    //                       (ActualQty_style * StdHoursPerUnit_style)
    //   MachineHours(day) = sum over styles produced that day of
    //                       (MachineCount_style * WorkHoursPerDay)
    //   DayEfficiency%    = EarnedHours(day) / MachineHours(day) * 100
    // StdHoursPerUnit_style = (sum of SAM across the style's non-manual
    // machine operations, from StyleOperationBreakdown/MachineType) / 60.
    // MachineCount_style = ProductionLineAllocation.NumberOfMachines for
    // (Line, style) if allocated, else the line's own default
    // (ProductionLine.NumberOfMachines).
    public class LineEfficiencyDayCellServiceModel
    {
        public int Day { get; set; }
        public string DayOfWeek { get; set; } = "";
        public bool IsHoliday { get; set; }
        public string? HolidayDescription { get; set; }
        public decimal? EfficiencyPercent { get; set; }
    }

    public class LineEfficiencyReportServiceModel
    {
        public string LineCode { get; set; } = null!;
        public string LineDescription { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public int DaysInMonth { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public decimal WorkHoursPerDay { get; set; }
        public List<LineEfficiencyDayCellServiceModel> Days { get; set; } = new();
        public decimal MonthlyAverageEfficiencyPercent { get; set; }
    }
}
