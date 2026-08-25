namespace ApparelPro.Data.Models.GeneralInventory
{
    // Replicates gi_stmst.dbf - General Inventory's per-store stock levels &
    // valuation, independent of any Buyer/Order (unlike OrderwiseStockMaster).
    public class GeneralStockMaster
    {
        public int Id { get; set; } // Internal database primary auto-increment identity seed

        public string StoreCode { get; set; } = null!; // STORE_CD
        public string ItemCode { get; set; } = null!;  // ITEM_CD - 22-char composite, same convention as OrderwiseStockMaster
        public string Unit { get; set; } = null!;       // UNIT

        public decimal QtyInHand { get; set; }      // QTY_IN_HD
        public decimal ShadowBalance { get; set; }   // SHDW_BAL
        public decimal Value { get; set; }            // VALUE - stock valuation amount
        public string Currency { get; set; } = null!; // CURR
        public decimal DamagedQuantity { get; set; }  // DAM_QTY

        public decimal ReorderLevel { get; set; }     // RO_LEVEL
        public decimal ReorderQuantity { get; set; }  // RO_QTY
        public decimal MinStock { get; set; }          // MIN_STOCK
        public decimal MaxStock { get; set; }          // MAX_STOCK
    }
}
