namespace ApparelPro.WebApi.APIModels.Reference
{
    public class CurrencyConversionAPIModel
    {
        public int Id { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? FromCurrencyName { get; set; }
        public string? ToCurrencyName { get; set; }
    }
}
