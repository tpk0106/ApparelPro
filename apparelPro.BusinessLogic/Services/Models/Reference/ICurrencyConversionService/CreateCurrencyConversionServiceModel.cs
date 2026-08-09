namespace apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyConversionService
{
    public class CreateCurrencyConversionServiceModel
    {
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
