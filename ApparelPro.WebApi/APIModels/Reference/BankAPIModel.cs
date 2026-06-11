using ApparelPro.Data.Models.References;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class BankAPIModel
    {        
        public string Name { get; set; }     
        public string BankCode { get; set; }
        public string? SwiftCode { get; set; }
        public string? TelephoneNos { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal LoanLimit { get; set; }        
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
