namespace ApparelPro.WebApi.APIModels.Production
{
    public class DailyProductionTimeTicketEntryAPIModel
    {
        public DateOnly Date { get; set; }
        public string LineCode { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string OperationCode { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string? NonProductiveHourCode { get; set; }
        public decimal NonProductiveHours { get; set; }
        public decimal WorkHours { get; set; }
    }
}
