using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderwiseInventory
{
    public class OrderwiseStockTransaction
    {
        public int Id { get; set; } // Auto-increment Identity Seed Primary Key

        public string DocumentNumber { get; set; } = null!; // xdocno (STRN / GIN / GRN number)
        public string TransactionType { get; set; } = null!; // "0S" = Stores Requisition Note (STRN), "4I" = GIN, etc.
        public DateTime TransactionDate { get; set; } // xdate

        public int BuyerCode { get; set; } // xbuyer
        public string Order { get; set; } = null!; // xorder
        public string DepartmentCode { get; set; } = null!; // xdept

        // Line-Item Material Tracking Parameters
        public string StockCode { get; set; } = null!; // Splits from character positions
        public string StoreCode { get; set; } = null!; // The "Basis" / Store the item was drawn from (matches OrderwiseStock.StoreCode)
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }

        public string CreatedByUsername { get; set; } = null!;

        // GIN traceability additions (see GinTraceabilityColumns migration)
        public decimal BalanceToReceive { get; set; } // bal_to_rec — STRN ('0S') rows only. Remaining unissued qty for this line; decremented by each GIN raised against it.
        public string? SourceDocumentNumber { get; set; } // GIN ('4I') rows only — the STRN doc number this issue was raised against.
        public decimal? Price { get; set; } // GIN rows only — snapshotted from OrderwiseStockMaster at issue time.
        public string? Currency { get; set; } // GIN rows only — snapshotted from OrderwiseStockMaster at issue time.
    }
}
