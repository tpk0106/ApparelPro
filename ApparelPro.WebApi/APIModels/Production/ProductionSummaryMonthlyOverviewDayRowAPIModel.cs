namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryMonthlyOverviewDayRowAPIModel
    {
        public DateOnly Date { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayDescription { get; set; }
        public List<ProductionSummaryMonthlyOverviewCellAPIModel> LineCells { get; set; } = new();
        public decimal TotalEstQuantity { get; set; }
        public decimal TotalActQuantity { get; set; }
        public decimal TotalCumEstQuantity { get; set; }
        public decimal TotalCumActQuantity { get; set; }
    }
}
