namespace ApparelPro.WebApi.APIModels.Reference
{
    public class CreateCurrencyConversionAPIModel
    {
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
