using ApparelPro.WebApi.APIModels.Production;

namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class ProductionProgressAPIModel
    {
        public string ContractSectionCode { get; set; } = null!;
        public List<SectionProgressAPIModel> Sections { get; set; } = new();
        public List<ProductionLineAllocationAPIModel> LineAllocations { get; set; } = new();
    }
}
