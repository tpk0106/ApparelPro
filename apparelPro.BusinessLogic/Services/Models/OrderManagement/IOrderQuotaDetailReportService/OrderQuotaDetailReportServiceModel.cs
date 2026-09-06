namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderQuotaDetailReportService
{
    // Replicates OD_ROQ1.PRG's "ORDER QUOTA REPORT". Legacy printed three column
    // layouts (sass1/sass2/sass3) that only differ in which already-filtered
    // Buyer/Order columns get hidden - a dot-matrix print convention, not different
    // business logic. This report always returns every column and lets the caller
    // narrow rows via the optional Buyer/Order filter instead.
    public class OrderQuotaDetailRowServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrderNo { get; set; } = null!; // new_order
        public string QuotaStatus { get; set; } = "N"; // qta_stat ('Q' = Quota, 'N' = Non-Quota)
        public string FromYearMonth { get; set; } = ""; // f_yymm
        public string ToYearMonth { get; set; } = ""; // t_yymm
        public string QuotaCategory { get; set; } = ""; // qta_cat
        public string QuotaType { get; set; } = ""; // qta_type
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }

    public class OrderQuotaDetailReportServiceModel
    {
        public int? BuyerCode { get; set; }
        public string? BuyerName { get; set; }
        public string? Order { get; set; }
        public List<OrderQuotaDetailRowServiceModel> Rows { get; set; } = new();
    }
}
