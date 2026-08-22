namespace ApparelPro.WebApi.APIModels.Production
{
    public class EstimatedProductionLineAllocationResultAPIModel
    {
        public EstimatedProductionLineAllocationAPIModel? Allocation { get; set; }
        public decimal UnallocatedQuantity { get; set; }
    }
}
