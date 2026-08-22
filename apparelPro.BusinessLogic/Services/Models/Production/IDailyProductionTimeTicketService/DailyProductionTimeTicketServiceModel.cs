namespace apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionTimeTicketService
{
    public class DailyProductionTimeTicketServiceModel
    {
        public List<DailyProductionTimeTicketEntryServiceModel> Entries { get; set; } = new();
        public List<EmployeeEfficiencySummaryServiceModel> EmployeeSummaries { get; set; } = new();
    }
}
