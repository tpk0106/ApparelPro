namespace ApparelPro.WebApi.Reports.Models
{
    public class MonthlyActualShipmentRowAPIModel
    {
        public string InvoiceNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string OrderNo { get; set; } = null!;
        public string StyleCode { get; set; } = null!;
        public DateTime ShipDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Balance { get; set; }
        public decimal Value { get; set; }
    }

    public class MonthlyActualShipmentsReportAPIModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<MonthlyActualShipmentRowAPIModel> Rows { get; set; } = new();
    }
}
