namespace apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyExchangeService
{
    public class CreateCurrencyExchangeServiceModel
    {
        public int Id { get; set; }
        public string? BaseCurrency { get; set; }
        public string? QuoteCurrency { get; set; }
        public decimal? Rate { get; set; }
        public DateTime ExchangeDate { get; set; } = DateTime.Now;
    }
}
