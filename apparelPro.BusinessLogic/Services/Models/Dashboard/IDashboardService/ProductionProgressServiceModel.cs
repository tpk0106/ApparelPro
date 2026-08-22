using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService;

namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    public class ProductionProgressServiceModel
    {
        public string ContractSectionCode { get; set; } = null!;
        public List<SectionProgressServiceModel> Sections { get; set; } = new();
        public List<ProductionLineAllocationServiceModel> LineAllocations { get; set; } = new();
    }
}
