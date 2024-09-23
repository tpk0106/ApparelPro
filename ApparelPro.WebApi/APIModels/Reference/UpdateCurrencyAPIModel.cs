using ApparelPro.Data.Models.References;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class UpdateCurrencyAPIModel 
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;        
        public string? CountryCode { get; set; }
        public string? Minor { get; set; }      
    }
}
