namespace ApparelPro.WebApi.APIModels.Production
{
    public class EmployeeEfficiencyRowAPIModel
    {
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = "";
        public decimal WorkHours { get; set; }
        public decimal EarnedHours { get; set; }
        public decimal NonProductiveHours { get; set; }
        public decimal OverEfficiencyPercent { get; set; }
        public decimal OperatorEfficiencyPercent { get; set; }
    }
}
