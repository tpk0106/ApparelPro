namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IOutstandingPurchaseOrderListReportService
{
    // Replicates OD_PLST1.PRG's "LIST OF OUTSTANDING P/O's - Date Wise" report - Start
    // Date/End Date range (required) + optional exact-match Basis filter, grouped by
    // Basis, listing every Purchase Order that has at least one outstanding
    // (nonzero-Balance) detail line.
    //
    // CORRECTED FROM LEGACY (2026-08-08, per user decision): legacy only inspected the
    // FIRST Buyer/Order/Type/Style detail group found on a P/O to decide "outstanding" -
    // a P/O with multiple groups where only a later group was outstanding would be
    // silently missed. This service checks EVERY detail group on the P/O, and lists
    // every outstanding group found (not just a single representative one).
    //
    // SCOPE NOTE: relies on PurchaseOrderHeader.CreatedDate (added 2026-08-08). P/O
    // headers created before that column existed have CreatedDate = null and are
    // correctly excluded from any date-range query, since there's no way to know when
    // they were actually created.
    public class OutstandingPurchaseOrderListReportServiceModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? BasisCode { get; set; }
        public List<OutstandingPurchaseOrderBasisGroupServiceModel> BasisGroups { get; set; } = new();
    }

    public class OutstandingPurchaseOrderBasisGroupServiceModel
    {
        public string BasisCode { get; set; } = "";
        public string BasisName { get; set; } = "";
        public List<OutstandingPurchaseOrderServiceModel> PurchaseOrders { get; set; } = new();
    }

    public class OutstandingPurchaseOrderServiceModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public DateOnly? CreatedDate { get; set; }
        public TimeOnly? CreatedTime { get; set; }
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public string? ProformaInvoiceNo { get; set; }
        public string CurrencyCode { get; set; } = "";
        public List<OutstandingPurchaseOrderGroupServiceModel> OutstandingGroups { get; set; } = new();
    }

    public class OutstandingPurchaseOrderGroupServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = "";
    }
}
