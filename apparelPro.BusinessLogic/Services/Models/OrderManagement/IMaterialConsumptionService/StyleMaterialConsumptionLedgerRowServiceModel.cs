namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService
{
    // Style-scoped consumption ledger row (mirrors the od_sacc3.dbf-backed
    // StyleMaterialConsumptionLedger entity) plus a joined Description, so the
    // consumption-ledger-grid can show the material name instead of a raw
    // 4-char ItemCode. Replaces returning the EF entity directly from
    // GetLedgerEntriesByStyleAsync.
    public class StyleMaterialConsumptionLedgerRowServiceModel
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
        // Joined from Suppliers by SupplierCode (see GetLedgerEntriesByStyleAsync) so
        // the grid can display the supplier's name instead of the raw numeric code.
        public string SupplierName { get; set; } = null!;
        public decimal TotalConsumption { get; set; }
        public decimal PercentageAllowance { get; set; }

        public bool IsAdditionalCost { get; set; }
        public bool CalculateConsumption { get; set; }
    }
}
