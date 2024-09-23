using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService
{
    public class CurrencyServiceModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public Country Country { get; set; } = new Country();
        public string? Minor { get; set; }
        public string? CurrencyDetails { get; set; }

    }
}
