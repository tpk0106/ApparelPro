namespace apparelPro.BusinessLogic.Services.Models.Reference.IBankService
{
    public class UpdateBankServiceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BankCode { get; set; }
        public string? SwiftCode { get; set; }
        public string? TelephoneNos { get; set; }
        public string CurrencyCode { get; set; }
        public int? addressId { get; set; }
        public decimal LoanAmount { get; set; } = 0;
    }
}
