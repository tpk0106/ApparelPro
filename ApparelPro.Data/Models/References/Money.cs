namespace ApparelPro.Data.Models.References
{
    public class Money
    {
        private readonly Currency _currency;
        public Money(decimal amount, Currency? currency)
        {
            if (currency == null)
            {
                throw new ArgumentNullException("Currency");
            }
            Amount = amount;
            _currency = currency;
        }

        public decimal Amount { get; set; }
        public Currency? Currency { get { return _currency; } }

        // public string QuoteCurrency { get; set; } = new Currency().Code!;
        public Currency? QuoteCurrency { get; set; }
    }
}
