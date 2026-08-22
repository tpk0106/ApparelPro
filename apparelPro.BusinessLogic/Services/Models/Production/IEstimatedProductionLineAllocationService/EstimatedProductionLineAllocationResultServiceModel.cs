namespace apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionLineAllocationService
{
    public class EstimatedProductionLineAllocationResultServiceModel
    {
        // Only ever holds one entry: EstimatedProductionLineAllocation is
        // keyed by Buyer+Style alone (matching legacy PR_ESTLN exactly), so
        // when the automatic scheduler's pass 2 would split across several
        // lines, each subsequent commit overwrites the same row - only the
        // last line processed actually survives. Preserved deliberately;
        // see ProductionLineSchedulingCalculator remarks.
        public EstimatedProductionLineAllocationServiceModel? Allocation { get; set; }
        public decimal UnallocatedQuantity { get; set; }
    }
}
