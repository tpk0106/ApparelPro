namespace ApparelPro.WebApi.APIModels.Reference
{
    public class BankAPIModel
    {        
        public string Name { get; set; }
        public string Description { get; set; }
        public string BankCode { get; set; }
        public string? SwiftCode { get; set; }
        public string? TelephoneNos { get; set; }
        public string CurrencyCode { get; set; }
        public int? AddressId { get; set; }
        public decimal LoanLimit { get; set; }
    }
}
