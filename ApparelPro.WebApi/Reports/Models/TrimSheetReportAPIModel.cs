namespace ApparelPro.WebApi.Reports.Models
{
    public class TrimSheetReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = "";

        public string Unit { get; set; } = "";
        public decimal StyleQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string BasisCode { get; set; } = "";
        public string BasisDescription { get; set; } = "";
        public string CurrencyCode { get; set; } = "";

        public List<TrimSheetLineAPIModel> Lines { get; set; } = new();
        public List<TrimSheetStockGroupAPIModel> StockGroupSubtotals { get; set; } = new();
        public List<TrimSheetSupplierTotalAPIModel> SupplierTotals { get; set; } = new();

        public decimal GrandTotalValue { get; set; }

        public bool SubContractSectionAvailable { get; set; }
        public bool ProductionLineSectionAvailable { get; set; }

        public TrimSheetProfitAPIModel? Profit { get; set; }
        public TrimSheetApprovalStampAPIModel? ApprovalStamp { get; set; }
    }

    public class TrimSheetLineAPIModel
    {
        public string StockCode { get; set; } = "";
        public string StockDescription { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Feature1 { get; set; } = "";
        public string Feature2 { get; set; } = "";
        public string Feature3 { get; set; } = "";
        public string Feature4 { get; set; } = "";

        public bool IsConsumptionCalculated { get; set; }
        public decimal QuantityPerGarment { get; set; }
        public string ConsumptionUnit { get; set; } = "";

        public decimal TotalConsumption { get; set; }
        public string ItemUnit { get; set; } = "";

        public decimal ConvertedUnitPrice { get; set; }
        public decimal Value { get; set; }

        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
    }

    public class TrimSheetStockGroupAPIModel
    {
        public string StockCode { get; set; } = "";
        public string StockDescription { get; set; } = "";
        public decimal SubtotalValue { get; set; }
        public decimal CostPerGarment { get; set; }
        public decimal PercentageOfUnitPrice { get; set; }
    }

    public class TrimSheetSupplierTotalAPIModel
    {
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public decimal TotalValue { get; set; }
    }

    public class TrimSheetProfitAPIModel
    {
        public decimal UnitPricePerGarment { get; set; }
        public decimal CostPerGarment { get; set; }
        public decimal CostPercentageOfUnitPrice { get; set; }
        public decimal EstimatedProfitPerGarment { get; set; }
        public decimal EstimatedProfitPercentage { get; set; }
    }

    public class TrimSheetApprovalStampAPIModel
    {
        public string ApprovedByUserId { get; set; } = "";
        public DateOnly ApprovedDate { get; set; }
    }
}
