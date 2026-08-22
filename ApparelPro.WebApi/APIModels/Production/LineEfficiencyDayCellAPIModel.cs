namespace ApparelPro.WebApi.APIModels.Production
{
    public class LineEfficiencyDayCellAPIModel
    {
        public int Day { get; set; }
        public string DayOfWeek { get; set; } = "";
        public bool IsHoliday { get; set; }
        public string? HolidayDescription { get; set; }
        public decimal? EfficiencyPercent { get; set; }
    }
}
