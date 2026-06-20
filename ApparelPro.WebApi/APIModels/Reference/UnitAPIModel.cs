using ApparelPro.Data.Models.References;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class UnitAPIModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
