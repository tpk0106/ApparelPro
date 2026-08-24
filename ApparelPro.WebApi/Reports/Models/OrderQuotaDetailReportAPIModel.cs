namespace ApparelPro.WebApi.Reports.Models
{
    public class OrderQuotaDetailRowAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrderNo { get; set; } = null!;
        public string QuotaStatus { get; set; } = "N";
        public string FromYearMonth { get; set; } = "";
        public string ToYearMonth { get; set; } = "";
        public string QuotaCategory { get; set; } = "";
        public string QuotaType { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }

    public class OrderQuotaDetailReportAPIModel
    {
        public int? BuyerCode { get; set; }
        public string? Order { get; set; }
        public List<OrderQuotaDetailRowAPIModel> Rows { get; set; } = new();
    }
}
