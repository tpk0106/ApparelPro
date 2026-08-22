namespace ApparelPro.WebApi.APIModels.Production
{
    public class EstimatedProductionScheduleRowAPIModel
    {
        public string LineCode { get; set; } = null!;
        public DateOnly EstStartDate { get; set; }
        public DateOnly EstEndDate { get; set; }
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public decimal EstimatedProductionPerDay { get; set; }
        public string Unit { get; set; } = null!;
        public decimal LeadTimeDays { get; set; }
        public decimal NumberOfDays { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateOnly ShipDate { get; set; }
        public int FloatDays { get; set; }
    }
}
