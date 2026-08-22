namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionLineAllocationResultAPIModel
    {
        public List<ProductionLineAllocationAPIModel> Commits { get; set; } = new();
        public decimal UnallocatedQuantity { get; set; }
    }
}
