namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    // Stage 5 - PartShipment has no "actually dispatched" flag anywhere;
    // every row counts as scheduled the moment it's created (per product
    // decision 2026-09-06, treated as "shipped" for this pipeline until a
    // real dispatch-confirmed field exists). ScheduledQuantity is
    // SUM(PartShipment.Quantity) for this style; TargetQuantity is the
    // style's own order Quantity. PartShipment carries no price/value field
    // of its own, so value is derived as quantity * Style.UnitPrice.
    public class ShipmentStageDetailServiceModel
    {
        public decimal TargetQuantity { get; set; }
        public decimal ScheduledQuantity { get; set; }
        public decimal TargetValue { get; set; }
        public decimal ScheduledValue { get; set; }
    }
}
