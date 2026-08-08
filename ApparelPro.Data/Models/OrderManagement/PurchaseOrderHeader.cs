using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderManagement
{
    public class PurchaseOrderHeader
    {
        public int Id { get; set; }

        public string PurchaseOrderNumber { get; set; } = null!; // po_no (e.g. "000004")
        public DateOnly? CreatedDate { get; set; }           // date - added 2026-08-08, nullable: rows created before this column existed have no value
        public TimeOnly? CreatedTime { get; set; }           // time - added 2026-08-08, same reasoning
        public string SupplierCode { get; set; } = null!;   // supp_cd
        public string StoreCode { get; set; } = null!;      // store_cd
        public string? ProformaInvoiceNo { get; set; }      // pi_no
        public DateOnly? ProformaInvoiceDate { get; set; }  // pi_date
        public string CurrencyCode { get; set; } = null!;   // curr

        // Multi-user concurrency editing lock flag
        public bool IsPoUsed { get; set; }                 // po_used
    }
}
