namespace apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService
{
    public class CreateCurrencyServiceModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;       
        public string CountryCode { get; set; }
        public string? Minor { get; set; }
    }
}
