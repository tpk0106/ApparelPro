namespace ApparelPro.Data.Models.Production
{
    // PR_ESTLN - pre-order production line planning. Keyed by Buyer+Style
    // only, matching the legacy table exactly (no Order/Type) - this is a
    // planning aid used before an order is fully confirmed, not a
    // per-order commitment like ProductionLineAllocation.
    public class EstimatedProductionLineAllocation
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
