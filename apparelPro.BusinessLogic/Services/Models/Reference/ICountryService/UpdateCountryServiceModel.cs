namespace apparelPro.BusinessLogic.Services.Models.Reference.ICountryService
{
    public class UpdateCountryServiceModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public byte[]? Flag { get; set; }
    }
}
