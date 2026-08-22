namespace ApparelPro.WebApi.APIModels.Production
{
    public class EstimatedProductionLineAllocationAPIModel
    {
        public int BuyerCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public decimal EstimatedProductionPerDay { get; set; }
        public string Unit { get; set; } = null!;
        public decimal LeadTimeDays { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateOnly ShipDate { get; set; }
        public string LineCode { get; set; } = null!;
        public decimal NumberOfDays { get; set; }
        public DateOnly EstimatedStartDate { get; set; }
        public DateOnly EstimatedEndDate { get; set; }
        public bool IsCritical { get; set; }
    }
}
