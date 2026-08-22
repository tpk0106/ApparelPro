namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService
{
    public class ProductionLineAllocationResultServiceModel
    {
        public List<ProductionLineAllocationServiceModel> Commits { get; set; } = new();
        public decimal UnallocatedQuantity { get; set; }
    }
}
