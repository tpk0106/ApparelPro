namespace apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyConversionService
{
    public class CurrencyConversionServiceModel
    {
        public int Id { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Value { get; set; }

        // Joined in from the Currency master (Currencies.Name) so the reference
        // grid can show "USD - US Dollar" instead of a bare code, same UX as the
        // Order Items Catalog's Stock Code column. Not persisted on
        // CurrencyConversion itself - read-only, populated by the service.
        public string? FromCurrencyName { get; set; }
        public string? ToCurrencyName { get; set; }
    }
}
