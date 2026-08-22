namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    public class DailyTrendSeriesServiceModel
    {
        public string SectionCode { get; set; } = null!;
        public string SectionDescription { get; set; } = string.Empty;
        public List<DailyTrendPointServiceModel> Points { get; set; } = new();
    }
}
