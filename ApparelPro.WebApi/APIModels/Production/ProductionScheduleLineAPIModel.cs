namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionScheduleLineAPIModel
    {
        public string LineCode { get; set; } = null!;
        public DateOnly EstimatedStartDate { get; set; }
        public DateOnly EstimatedEndDate { get; set; }
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrder { get; set; } = null!;
        public decimal EstimatedProductionPerDay { get; set; }
        public string Unit { get; set; } = null!;
        public decimal LeadTimeDays { get; set; }
        public decimal NumberOfDays { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateOnly? ShipDate { get; set; }
        public int? FloatDays { get; set; }
    }
}
