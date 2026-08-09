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

        // GTN traceability additions (see GtnTraceabilityColumns migration). GTN ('6T'/'1T')
        // rows only — mirrors legacy IN_GTN1.PRG's t_buyer/t_order fields written onto BOTH
        // legs of a transfer so each leg can be printed/traced back to its counterpart without
        // a self-join: a '6T' (Transfer-Out) row's Buyer/Order is the source, and
        // CounterpartyBuyerCode/CounterpartyOrder is the destination; a '1T' (Transfer-In) row
        // is the mirror image.
        public int? CounterpartyBuyerCode { get; set; } // t_buyer
        public string? CounterpartyOrder { get; set; } // t_order

        // SRN traceability addition (see SrnTraceabilityColumns migration). SRN ('7S')
        // rows only — mirrors legacy IN_SRN1.PRG's supp_cd field, recording which
        // supplier the returned stock went back to.
        public int? SupplierCode { get; set; } // supp_cd

        // AIN traceability additions (see AddAdditionalIssueNoteSupport migration). AIN
        // ('4X') rows only — legacy IN_AIN1.PRG/IN_AIN3.PRG overload the shared t_buyer/
        // t_order columns to carry the Sub-Contractor code and Additional Process code
        // instead of a counterparty Buyer/Order (that's what CounterpartyBuyerCode/
        // CounterpartyOrder above are for, GTN's own use of the same two legacy columns) —
        // different domain, different type (Sub-Contractor's code is a 6-char string, not
        // an int Buyer code), so AIN gets its own two dedicated columns rather than reusing
        // those.
        public string? SubContractorCode { get; set; } // t_buyer (AIN's own meaning, not GTN's)
        public string? AdditionalProcessCode { get; set; } // t_order (AIN's own meaning, not GTN's)
    }
}
