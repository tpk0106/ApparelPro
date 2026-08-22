namespace ApparelPro.WebApi.APIModels.Production
{
    public class DailyProductionTimeTicketAPIModel
    {
        public List<DailyProductionTimeTicketEntryAPIModel> Entries { get; set; } = new();
        public List<EmployeeEfficiencySummaryAPIModel> EmployeeSummaries { get; set; } = new();
    }
}
