namespace ApparelPro.WebApi.APIModels.Production
{
    public class CreateHolidayAPIModel
    {
        public DateOnly Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
