namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService
{
    public class AutomaticAllocateProductionLineServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrder { get; set; } = null!;
        public decimal EstimatedProductionPerDay { get; set; }
        public string Unit { get; set; } = null!;
        public decimal LeadTimeDays { get; set; }
        public int NumberOfMachines { get; set; }
    }
}
