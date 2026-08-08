namespace ApparelPro.WebApi.Reports.Models
{
    public class OrderDetailReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public DateOnly OrderDate { get; set; }
        public string Unit { get; set; } = "";
        public string CurrencyCode { get; set; } = "";

        public List<OrderDetailStyleAPIModel> Styles { get; set; } = new();

        public decimal GrandTotalValue { get; set; }
    }

    public class OrderDetailStyleAPIModel
    {
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = "";

        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public List<OrderDetailPartShipmentAPIModel> PartShipments { get; set; } = new();

        public decimal TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class OrderDetailPartShipmentAPIModel
    {
        public string NewOrder { get; set; } = "";
        public string DestinationCode { get; set; } = "";
        public DateOnly ShipDate { get; set; }

        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Value { get; set; }
    }
}
