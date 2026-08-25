namespace ApparelPro.Data.Models.GeneralInventory
{
    // Replicates gi_sttr.dbf - the transaction ledger every General Inventory note type
    // (GRN/GIN/GTN/RTN/SRN/DGN/SAN) writes to. Buyer/Order/PoNumber are optional
    // tie-backs since most General Inventory movement isn't linked to a specific order.
    public class GeneralStockTransaction
    {
        public int Id { get; set; } // Internal database primary auto-increment identity seed

        // Legacy ID field (e.g. "3A") - a note-type/action code, not a numeric key.
        public string TransactionTypeCode { get; set; } = null!; // ID
        public string DocumentNumber { get; set; } = null!;      // DOCNO

        public DateOnly TransactionDate { get; set; } // DATE
        public TimeOnly? TransactionTime { get; set; } // TIME

        public string? InvoiceNumber { get; set; } // INV_NO
        public string StoreCode { get; set; } = null!; // STORE_CD
        public string ItemCode { get; set; } = null!;  // ITEM_CD
        public string Unit { get; set; } = null!;       // UNIT
        public decimal Quantity { get; set; }            // QTY

        public string? SupplierCode { get; set; } // SUPP_CD
        public decimal Price { get; set; }          // PRICE
        public string? Currency { get; set; }       // CURR
        public decimal? ExchangeRate { get; set; }  // EX_RATE

        public int? BuyerCode { get; set; } // BUYER - optional tie-back to an order
        public string? Order { get; set; }  // ORDER
        public string? PoNumber { get; set; } // PO_NO

        // Legacy GI_STRN1.PRG reuses the BUYER column to store the "To Department" code
        // for SRN-type transactions - not modeled that way here since BuyerCode above is
        // a real int used for its own purpose (GRN/GIN order tie-backs). A dedicated
        // nullable column keeps both concepts type-safe, per explicit decision 2026-08-25.
        public string? DepartmentCode { get; set; }

        // Legacy GI_GIN1.PRG reuses the PO_NO column on an SRN row to store the GIN number
        // that fulfilled it - not modeled onto PoNumber above since that column keeps its
        // own meaning (a real supplier PO tie-back on GRN rows). A dedicated nullable
        // column for "the other document number this row is linked to" keeps both concepts
        // unambiguous, per explicit decision 2026-08-25.
        public string? LinkedDocumentNumber { get; set; }
    }
}
