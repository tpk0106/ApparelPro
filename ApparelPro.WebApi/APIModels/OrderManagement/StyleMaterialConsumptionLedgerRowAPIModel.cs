namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class StyleMaterialConsumptionLedgerRowAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string Size { get; set; } = null!;
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Feature1 { get; set; } = null!;
        public string Feature2 { get; set; } = null!;
        public string Feature3 { get; set; } = null!;
        public string Feature4 { get; set; } = null!;

        public string StoreCode { get; set; } = null!;
        public string ConsumptionUnit { get; set; } = null!;
        public string ItemUnit { get; set; } = null!;
        public decimal QuantityPerGarment { get; set; }
        public string SupplierCode { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
        public decimal TotalConsumption { get; set; }
        public decimal PercentageAllowance { get; set; }

        public bool IsAdditionalCost { get; set; }
        public bool CalculateConsumption { get; set; }
    }
}
