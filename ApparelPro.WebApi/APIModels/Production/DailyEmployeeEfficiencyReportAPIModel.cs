namespace ApparelPro.WebApi.APIModels.Production
{
    public class DailyEmployeeEfficiencyReportAPIModel
    {
        public DateOnly Date { get; set; }
        public string? LineCode { get; set; }
        public string? LineDescription { get; set; }
        public List<EmployeeEfficiencyRowAPIModel> Rows { get; set; } = new();
    }
}
