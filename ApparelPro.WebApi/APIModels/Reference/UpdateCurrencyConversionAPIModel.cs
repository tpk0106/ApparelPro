namespace ApparelPro.WebApi.APIModels.Reference
{
    public class UpdateCurrencyConversionAPIModel
    {
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
