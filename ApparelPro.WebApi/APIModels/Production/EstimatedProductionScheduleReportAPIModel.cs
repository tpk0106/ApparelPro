namespace ApparelPro.WebApi.APIModels.Production
{
    public class EstimatedProductionScheduleReportAPIModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public List<EstimatedProductionScheduleRowAPIModel> Rows { get; set; } = new();
    }
}
