namespace ApparelPro.Data.Models.References
{
    public class CurrencyExchange
    {
        public int Id { get; set; }
        public string? BaseCurrency { get; set; }
        public string? QuoteCurrency { get; set; }
        public decimal? Rate { get; set; }
        public DateTime ExchangeDate { get; set; }
    }
}
