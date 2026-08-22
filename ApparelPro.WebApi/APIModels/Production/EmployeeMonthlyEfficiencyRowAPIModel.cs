namespace ApparelPro.WebApi.APIModels.Production
{
    public class EmployeeMonthlyEfficiencyRowAPIModel
    {
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = "";
        public List<EmployeeMonthlyEfficiencyDayCellAPIModel> Days { get; set; } = new();
        public decimal MonthlyAverageEfficiencyPercent { get; set; }
    }
}
