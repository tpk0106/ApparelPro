namespace ApparelPro.WebApi.Reports.Models
{
    public class ShipmentStatusInvoiceLineAPIModel
    {
        public decimal QuantityShipped { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; } = null!;
    }

    public class ShipmentStatusRowAPIModel
    {
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrderNo { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string DestinationCode { get; set; } = null!;
        public List<ShipmentStatusInvoiceLineAPIModel> InvoiceLines { get; set; } = new();
        public decimal TotalQuantityShipped { get; set; }
        public decimal BalanceToShip { get; set; }
    }

    public class ShipmentStatusReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public List<ShipmentStatusRowAPIModel> Rows { get; set; } = new();
    }
}
