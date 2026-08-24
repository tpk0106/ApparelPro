namespace ApparelPro.WebApi.Reports.Models
{
    public class ScheduledShipmentRowAPIModel
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
        public string DestinationCode { get; set; } = null!;
        public DateTime ShipDate { get; set; }
    }

    public class ScheduledShipmentsReportAPIModel
    {
        public int? BuyerCode { get; set; }
        public string? Order { get; set; }
        public List<ScheduledShipmentRowAPIModel> Rows { get; set; } = new();
    }
}
