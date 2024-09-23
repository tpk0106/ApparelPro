namespace ApparelPro.Data.Models.References
{
    public class Bank
    {
        public int Id { get; set; }
        public string? BankCode { get; set; }
        public string? Name { get; set; }
        public string? TelephoneNos { get; set; }
        public string? SwiftCode { get; set; }
        public int? AddressId { get; set; }        

        //  public List<Address>? Addresses { get; set; }
        public decimal LoanLimit { get; set; }
        public string? CurrencyCode { get; set; }
    }
}
