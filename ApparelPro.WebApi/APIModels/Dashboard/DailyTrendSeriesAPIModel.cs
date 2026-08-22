namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class DailyTrendSeriesAPIModel
    {
        public string SectionCode { get; set; } = null!;
        public string SectionDescription { get; set; } = string.Empty;
        public List<DailyTrendPointAPIModel> Points { get; set; } = new();
    }
}
