namespace apparelPro.BusinessLogic.Services.Models.Production.IManpowerRequirementReportService
{
    // Replicates PR_REP3.PRG's "MANPOWER REQUIREMENT" report (Reports ->
    // Manpower Requirement). Same StyleOperationBreakdown rows as the
    // Operation Breakdown report, but grouped by Machine Type (split into
    // machine-operated vs. manual) rather than Component. This is the report
    // that actually consumes the total-SAM/output-projection calculation
    // that turned out to be dead code (never printed) in Operation Breakdown -
    // here it's the report's whole point.
    public class ManpowerMachineTimeRowServiceModel
    {
        public string MachineTypeCode { get; set; } = null!;
        public string MachineTypeDescription { get; set; } = "";
        public decimal TotalSam { get; set; }
    }

    public class ManpowerRequirementReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string? LineCode { get; set; }

        public List<ManpowerMachineTimeRowServiceModel> MachineRows { get; set; } = new();
        public List<ManpowerMachineTimeRowServiceModel> ManualRows { get; set; } = new();

        // Machine-only SAM sum ("sub_tot" in legacy) - the bottleneck figure
        // the whole output projection is based on.
        public decimal TotalMachineTimeSam { get; set; }

        // Machine + manual SAM sum ("main_tot" in legacy).
        public decimal GrandTotalSam { get; set; }

        public decimal PcsPerMachineAt100 { get; set; }
        public decimal PcsPerMachineAtEff1 { get; set; }

        // Null when Eff2 is configured as 0 - legacy skips this line/section
        // entirely in that case rather than showing a zero.
        public decimal? PcsPerMachineAtEff2 { get; set; }

        // NumberOfMachines summed across every ProductionLineAllocation for
        // (LineCode, style) when a Line was given, otherwise the factory-wide
        // ProductionDefaultMachineCountPerLine default.
        public decimal MachineCount { get; set; }

        public decimal TargetOutputAt100 { get; set; }
        public decimal TargetOutputAtEff1 { get; set; }
        public decimal? TargetOutputAtEff2 { get; set; }

        public decimal EstimatedStandardHours { get; set; }

        public decimal Eff1Percent { get; set; }
        public decimal Eff2Percent { get; set; }
        public decimal WorkHoursPerDay { get; set; }
    }
}
