namespace ApparelPro.Data.Models.GeneralInventory
{
    // Replicates gi_pohed.dbf - General Inventory's own Purchase Order header, distinct
    // from OrderManagement.SupplierPurchaseOrder (od_pohed) which is Buyer/Order-specific.
    public class GeneralPurchaseOrder
    {
        public string PoNumber { get; set; } = null!; // PO_NO

        public string SupplierCode { get; set; } = null!; // SUPP_CD
        public DateOnly? OrderDate { get; set; }  // DATE
        public TimeOnly? OrderTime { get; set; }  // TIME
        public string? BasisCode { get; set; }     // BASIS
        public string? ProformaInvoiceNo { get; set; } // PI_NO
        public DateOnly? ProformaInvoiceDate { get; set; } // PI_DATE
        public string? CurrencyCode { get; set; } // CURR
        public string? UserId { get; set; }        // USERID
    }
}
