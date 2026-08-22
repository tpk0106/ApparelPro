namespace ApparelPro.WebApi.APIModels.Production
{
    public class MonthlyEmployeeEfficiencyReportAPIModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int DaysInMonth { get; set; }
        public decimal WorkHoursPerDay { get; set; }
        public List<EmployeeMonthlyEfficiencyRowAPIModel> Rows { get; set; } = new();
    }
}
