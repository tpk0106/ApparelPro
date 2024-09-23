namespace ApparelPro.Data.Models.References
{
    public class CurrencyConversion
    {
        public int Id { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Value { get; set; }
    }
}
