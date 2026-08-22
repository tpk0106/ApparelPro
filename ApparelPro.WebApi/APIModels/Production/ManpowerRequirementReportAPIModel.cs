namespace ApparelPro.WebApi.APIModels.Production
{
    public class ManpowerRequirementReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string? LineCode { get; set; }

        public List<ManpowerMachineTimeRowAPIModel> MachineRows { get; set; } = new();
        public List<ManpowerMachineTimeRowAPIModel> ManualRows { get; set; } = new();

        public decimal TotalMachineTimeSam { get; set; }
        public decimal GrandTotalSam { get; set; }

        public decimal PcsPerMachineAt100 { get; set; }
        public decimal PcsPerMachineAtEff1 { get; set; }
        public decimal? PcsPerMachineAtEff2 { get; set; }

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
