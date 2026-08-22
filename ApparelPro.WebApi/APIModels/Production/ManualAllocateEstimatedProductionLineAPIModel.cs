namespace ApparelPro.WebApi.APIModels.Production
{
    public class ManualAllocateEstimatedProductionLineAPIModel
    {
        public int BuyerCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public decimal EstimatedProductionPerDay { get; set; }
        public string Unit { get; set; } = null!;
        public decimal LeadTimeDays { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateOnly ShipDate { get; set; }
        public string LineCode { get; set; } = null!;
        public DateOnly EstimatedStartDate { get; set; }
    }
}
