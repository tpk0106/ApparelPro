namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IScheduledShipmentsReportService
{
    // Replicates OD_RSHP1.PRG's "SCHEDULE SHIPMENT DETAIL REPORT". Legacy printed three
    // column layouts (sha1/sha2/sha3) that only differ in which already-filtered
    // Buyer/Order columns get hidden - a dot-matrix print convention, not different
    // business logic. This report always returns every column and lets the caller
    // narrow rows via the optional Buyer/Order filter instead.
    public class ScheduledShipmentRowServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrderNo { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        // Shown as the raw code only - Destination's current schema (keyed by
        // Id+CountryCode) has no field matching PartShipment.DestinationCode to
        // resolve a display name from (a known schema gap, see the Order
        // Management specification's data-integrity notes).
        public string DestinationCode { get; set; } = null!;
        public DateTime ShipDate { get; set; }
    }

    public class ScheduledShipmentsReportServiceModel
    {
        public int? BuyerCode { get; set; }
        public string? BuyerName { get; set; }
        public string? Order { get; set; }
        public List<ScheduledShipmentRowServiceModel> Rows { get; set; } = new();
    }
}
