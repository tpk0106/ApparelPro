namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsDeclarationService
{
    public class CustomsDeclarationLineTaxServiceModel
    {
        public int Id { get; set; }
        public string? TaxCode { get; set; }
        public string? BaseCode { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Exempted { get; set; }
        public decimal? Payable { get; set; }
    }
}
