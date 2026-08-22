namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionScheduleReportService
{
    // Replicates PR_MSCHD.PRG's "PRODUCTION SCHEDULE" report (Reports -> A.
    // Production Schedule). One row per ProductionLineAllocation whose
    // EstimatedStartDate falls within the given date range.
    public class ProductionScheduleLineServiceModel
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

        // From PartShipments, matched on Buyer+Order+Type+Style+ShipmentOrder
        // (od_part seek). Null if no matching shipment row exists.
        public DateOnly? ShipDate { get; set; }

        // ShipDate - EstimatedEndDate, in days. Negative means the line is
        // projected to finish producing AFTER the goods are due to ship.
        public int? FloatDays { get; set; }
    }

    public class ProductionScheduleReportServiceModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public List<ProductionScheduleLineServiceModel> Lines { get; set; } = new();
    }
}
