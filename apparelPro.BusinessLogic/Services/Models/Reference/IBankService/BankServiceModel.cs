using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IBankService
{
    public class BankServiceModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }        
        public string? BankCode { get; set; }
        public string? SwiftCode { get; set; }
        public string? TelephoneNos { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal LoanLimit { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();        
    }
}
