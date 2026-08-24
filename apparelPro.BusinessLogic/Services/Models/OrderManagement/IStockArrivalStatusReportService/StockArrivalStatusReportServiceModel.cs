namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IStockArrivalStatusReportService
{
    // Replicates OD_STARV.PRG's "STOCK ARRIVAL STATUS REPORT" - for every material
    // budgeted against the given Buyer+Order (od_sacc2), shows how much has arrived so
    // far and the individual Purchase Order lines raised to cover it.
    //
    // DEFECT FIXED vs legacy: OD_STARV.PRG seeks od_pohed (for StoreCode/SupplierCode)
    // exactly once per item, using the FIRST matching od_podet row's prch_no, then reuses
    // that same stale header context for every PO line printed afterward even when later
    // lines belong to a different purchase order (a different store/supplier). This
    // rebuild resolves each PO line's own header independently instead.
    public class StockArrivalPoLineServiceModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public string StoreCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public DateTime? ExpectedDate { get; set; }
        // Null when ExpectedDate is null (legacy prints "Not Specified" with no delay
        // figure in that case) - matches "m_date - c_tod(exp_date) + 1".
        public int? DelayDays { get; set; }
        public decimal SupplierReturnQuantity { get; set; }
    }

    public class StockArrivalItemServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal OrderedQuantity { get; set; } // od_sacc2.tot_con
        // Sum of OrderwiseStock.ToDateReceived (unit-converted to Unit) across every
        // distinct store this item's PO lines were raised against.
        public decimal TotalReceivedQuantity { get; set; }
        public decimal BalanceToReceive { get; set; }
        public List<StockArrivalPoLineServiceModel> PurchaseOrderLines { get; set; } = new();
    }

    public class StockArrivalStatusReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public DateTime AsOfDate { get; set; }
        public decimal TotalOrderQuantity { get; set; }
        public string Unit { get; set; } = "";
        public List<StockArrivalItemServiceModel> Items { get; set; } = new();
    }
}
