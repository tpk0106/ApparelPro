namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionScheduleReportAPIModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public List<ProductionScheduleLineAPIModel> Lines { get; set; } = new();
    }
}
