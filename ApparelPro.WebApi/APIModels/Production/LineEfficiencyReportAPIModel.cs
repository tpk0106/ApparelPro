namespace ApparelPro.WebApi.APIModels.Production
{
    public class LineEfficiencyReportAPIModel
    {
        public string LineCode { get; set; } = null!;
        public string LineDescription { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public int DaysInMonth { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public decimal WorkHoursPerDay { get; set; }
        public List<LineEfficiencyDayCellAPIModel> Days { get; set; } = new();
        public decimal MonthlyAverageEfficiencyPercent { get; set; }
    }
}
