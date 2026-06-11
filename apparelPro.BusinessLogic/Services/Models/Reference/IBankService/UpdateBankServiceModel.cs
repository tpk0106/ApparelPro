using ApparelPro.Data.Models.References;
using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IBankService
{
    public class UpdateBankServiceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }        
        [Required]
        public string BankCode { get; set; }
        public string? SwiftCode { get; set; }
        public string? TelephoneNos { get; set; }
        public string CurrencyCode { get; set; }        
        public decimal LoanLimit { get; set; } = 0;
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
