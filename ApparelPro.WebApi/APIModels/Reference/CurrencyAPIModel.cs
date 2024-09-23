using ApparelPro.Data.Models.References;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class CurrencyAPIModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty ;
        public Country Country { get; set; } = new Country();
        public string? Minor { get; set; }
        public string? CurrencyDetails { get; set; }
    }
}
