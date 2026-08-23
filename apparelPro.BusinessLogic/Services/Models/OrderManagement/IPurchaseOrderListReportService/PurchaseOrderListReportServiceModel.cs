using System;
using System.Collections.Generic;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderListReportService
{
    // Replicates OD_POLST.PRG's "PURCHASE ORDER LIST" print program (legacy menu item
    // "List of P/O's"), scoped to a single Supplier Purchase Order number - lists every
    // line item on that P/O, each resolved back to the Buyer/Order/Type/Style it was
    // raised against.
    //
    // SCOPE NOTE (2026-08-08): legacy prints the P/O's creation Date and Time
    // (od_pohed->date, od_pohed->time) in its header. SupplierPurchaseOrder (the modern
    // equivalent of od_pohed) has no column for either - confirmed by tracing every
    // place SupplierPurchaseOrderService.cs writes a header, which sets only
    // SupplierCode/StoreCode/ProformaInvoiceNo/ProformaInvoiceDate/CurrencyCode, never
    // a creation date/time. Omitted from this report rather than fabricated.
    public class PurchaseOrderListReportServiceModel
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public string? ProformaInvoiceNo { get; set; }
        public DateOnly? ProformaInvoiceDate { get; set; }
        public string CurrencyCode { get; set; } = "";

        public List<PurchaseOrderListLineServiceModel> Lines { get; set; } = new();
    }

    public class PurchaseOrderListLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public decimal OrderQuantity { get; set; }
        public string OrderUnit { get; set; } = "";
        public decimal UnitPrice { get; set; }

        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
    }
}
