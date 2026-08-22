namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService
{
    // Manual mode (PR_ESTM1.PRG, m_autoalc = 2): the user picks the line and
    // start date directly - the scheduler's two-pass search never runs, but
    // day-count/holiday-expansion math is still identical to automatic mode.
    public class ManualAllocateProductionLineServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrder { get; set; } = null!;
        public string LineCode { get; set; } = null!;
        public decimal EstimatedProductionPerDay { get; set; }
        public string Unit { get; set; } = null!;
        public decimal LeadTimeDays { get; set; }
        public int NumberOfMachines { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateOnly EstimatedStartDate { get; set; }
    }
}
