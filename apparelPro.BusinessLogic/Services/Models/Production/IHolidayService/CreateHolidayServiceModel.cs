namespace apparelPro.BusinessLogic.Services.Models.Production.IHolidayService
{
    public class CreateHolidayServiceModel
    {
        public DateOnly Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
