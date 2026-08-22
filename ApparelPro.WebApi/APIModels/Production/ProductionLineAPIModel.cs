namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionLineAPIModel
    {
        public string LineCode { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public int NumberOfMachines { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public decimal LineCostPerDay { get; set; }
        public int MinimumProductionPerOrder { get; set; }
        public string UnitCode { get; set; } = null!;
        public DateOnly? NextAllocationDate { get; set; }
        public DateOnly? EstimatedNextAllocationDate { get; set; }
    }
}
