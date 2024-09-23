namespace ApparelPro.WebApi.APIModels.Reference
{
    public class CreateCurrencyAPIModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;      
        public string CountryCode { get; set; } = string.Empty;
        public string? Minor { get; set; }
    }
}
