namespace ApparelPro.Data.Models.GeneralInventory
{
    // Replicates gi_podet.dbf - line items for a GeneralPurchaseOrder.
    public class GeneralPurchaseOrderDetails
    {
        public int Id { get; set; } // Internal database primary auto-increment identity seed

        public string PoNumber { get; set; } = null!; // PO_NO - FK to GeneralPurchaseOrder
        public string StoreCode { get; set; } = null!; // STORE_CD
        public string ItemCode { get; set; } = null!;  // ITEM_CD
        public string? RefNo { get; set; }              // REF_NO
        public string Unit { get; set; } = null!;        // UNIT
        public decimal OrderedQuantity { get; set; }      // ORD_QTY
        public decimal Price { get; set; }                 // PRICE
        public DateOnly? ExpectedDate { get; set; }        // EXP_DATE
        public decimal Balance { get; set; }                // BALANCE
    }
}
