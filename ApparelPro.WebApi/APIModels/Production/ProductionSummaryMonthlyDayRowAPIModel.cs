namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryMonthlyDayRowAPIModel
    {
        public DateOnly Date { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayDescription { get; set; }
        public List<ProductionSummaryMonthlySubRowAPIModel> SubRows { get; set; } = new();
    }
}
