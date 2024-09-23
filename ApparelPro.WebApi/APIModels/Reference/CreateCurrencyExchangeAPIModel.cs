﻿namespace ApparelPro.WebApi.APIModels.Reference
{
    public class CreateCurrencyExchangeAPIModel
    {
        public int Id { get; set; }
        public string? BaseCurrency { get; set; }
        public string? QuoteCurrency { get; set; }
        public decimal? Rate { get; set; }
        public DateTime ExchangeDate { get; set; } = DateTime.Now;
    }
}
