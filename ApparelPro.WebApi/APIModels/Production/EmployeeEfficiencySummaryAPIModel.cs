namespace ApparelPro.WebApi.APIModels.Production
{
    public class EmployeeEfficiencySummaryAPIModel
    {
        public string EmployeeCode { get; set; } = null!;
        public decimal WorkHours { get; set; }
        public decimal NonProductiveHours { get; set; }
        public decimal EarnedMinutes { get; set; }
        public decimal OverEfficiencyPercent { get; set; }
        public decimal OperatorEfficiencyPercent { get; set; }
    }
}
