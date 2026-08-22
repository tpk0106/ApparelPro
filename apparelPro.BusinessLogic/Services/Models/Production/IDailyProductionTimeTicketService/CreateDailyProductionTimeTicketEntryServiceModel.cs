namespace apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionTimeTicketService
{
    public class CreateDailyProductionTimeTicketEntryServiceModel
    {
        public string EmployeeCode { get; set; } = null!;
        public string OperationCode { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string? NonProductiveHourCode { get; set; }
        public decimal NonProductiveHours { get; set; }
        public decimal WorkHours { get; set; }
    }
}
