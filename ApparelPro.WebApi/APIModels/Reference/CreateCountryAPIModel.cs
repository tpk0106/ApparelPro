using ApparelPro.WebApi.CustomAttributes;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class CreateCountryAPIModel
    {
        public int Id { get; set; }
        [LettersOnlyValidation()]
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public byte[]? Flag { get; set; }
    }
}
