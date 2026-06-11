namespace ApparelPro.Data.Models.References
{
    public class Bank
    {
        public int Id { get; set; }
        public string BankCode { get; set; }
        public string? Name { get; set; }
        public string? TelephoneNos { get; set; }
        public string? SwiftCode { get; set; }        
        public ICollection<Address> Addresses { get; set; } = new List<Address>();        
        public decimal LoanLimit { get; set; }
        public string? CurrencyCode { get; set; }
    }
}
