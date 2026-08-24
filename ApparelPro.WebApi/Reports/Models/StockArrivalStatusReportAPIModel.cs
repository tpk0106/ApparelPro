namespace ApparelPro.WebApi.Reports.Models
{
    public class StockArrivalPoLineAPIModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public string StoreCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public DateTime? ExpectedDate { get; set; }
        public int? DelayDays { get; set; }
        public decimal SupplierReturnQuantity { get; set; }
    }

    public class StockArrivalItemAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal OrderedQuantity { get; set; }
        public decimal TotalReceivedQuantity { get; set; }
        public decimal BalanceToReceive { get; set; }
        public List<StockArrivalPoLineAPIModel> PurchaseOrderLines { get; set; } = new();
    }

    public class StockArrivalStatusReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public DateTime AsOfDate { get; set; }
        public decimal TotalOrderQuantity { get; set; }
        public string Unit { get; set; } = "";
        public List<StockArrivalItemAPIModel> Items { get; set; } = new();
    }
}
