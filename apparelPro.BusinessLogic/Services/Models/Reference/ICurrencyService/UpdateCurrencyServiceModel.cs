using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService
{
    public class UpdateCurrencyServiceModel
    {
        public int Id { get; set; }
        public string? Code { get; set; } 
        public string? Name { get; set; } 
        public string? CountryCode {  get; set; }      
        public string? Minor { get; set; }
    }
}
