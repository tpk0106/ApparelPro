namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService
{
    public class ProductionLineAllocationServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrder { get; set; } = null!;
        public string LineCode { get; set; } = null!;

        public decimal EstimatedProductionPerDay { get; set; }
        public decimal TotalQuantity { get; set; }
        public string Unit { get; set; } = null!;
        public decimal LeadTimeDays { get; set; }
        public int NumberOfMachines { get; set; }
        public decimal CostPerDay { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public decimal NumberOfDays { get; set; }
        public DateOnly OriginalEstimatedStartDate { get; set; }
        public DateOnly OriginalEstimatedEndDate { get; set; }
        public DateOnly EstimatedStartDate { get; set; }
        public DateOnly EstimatedEndDate { get; set; }
        public bool IsCritical { get; set; }
    }
}
