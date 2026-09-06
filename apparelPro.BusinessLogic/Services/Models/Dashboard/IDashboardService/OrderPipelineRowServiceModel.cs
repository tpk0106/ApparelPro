namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    // One row = one (BuyerCode, Order, TypeCode, StyleCode) - the same
    // granularity every other module (ColorSizeDetails, Style Operation
    // Breakdown, Part Shipment, ...) already tracks its own data at, so no
    // new identity concept is introduced here.
    //
    // Stage values: 0 Merchandising, 1 Approval, 2 Supplier PO, 3 GRN,
    // 4 Production, 5 Shipment, 6 Complete (fully shipped).
    //
    // DaysInStage/IsOverdue (added 2026-09-06): backed by the new
    // OrderPipelineStageHistory audit table - the first "reconcile on read"
    // pass in OrderPipelineService stamps a row the first time a style is
    // seen at a given stage, since nothing else in the schema tracked this.
    // A style computed at a stage for the first time this table has ever
    // seen it will show 0 days in stage, not its true history.
    public class OrderPipelineRowServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }

        public int Stage { get; set; }
        public int DaysInStage { get; set; }
        public bool IsOverdue { get; set; }

        public MerchandisingStageDetailServiceModel Merchandising { get; set; } = new();
        public ApprovalStageDetailServiceModel Approval { get; set; } = new();
        public SupplierPoStageDetailServiceModel SupplierPo { get; set; } = new();
        public GrnStageDetailServiceModel Grn { get; set; } = new();
        public ProductionStageDetailServiceModel Production { get; set; } = new();
        public ShipmentStageDetailServiceModel Shipment { get; set; } = new();
    }
}
