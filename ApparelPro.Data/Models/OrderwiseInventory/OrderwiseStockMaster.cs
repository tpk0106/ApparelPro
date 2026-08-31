using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderwiseInventory
{
    // Replicates in_stmst.dbf (Master Costing & Valuation Stocks Ledger)
    public class OrderwiseStockMaster
    {
        public int Id { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public decimal Price { get; set; }
        // 🚀 ADDED: Tracks cumulative reserved quantities (Maps to legacy req_qty)
        public decimal RequisitionedQuantity { get; set; }

        // GIN traceability addition: running total issued via GIN (legacy issd_qty).
        // Balance-remaining is derived as OrderedQuantity - IssuedQuantity rather than
        // persisted as a second mutable total (avoids the legacy issd_qty/bal_qty drift risk).
        public decimal IssuedQuantity { get; set; }

        // GRN traceability addition: running total received via GRN (legacy rcvd_qty).
        // Also incremented by ARN ("Additional Goods Receipts Note") - legacy IN_ARN4.PRG
        // writes to this same rcvd_qty field, since both represent "goods received against
        // this order," just from a Supplier (GRN) or a Sub-Contractor (ARN).
        public decimal ReceivedQuantity { get; set; }

        // RTN traceability addition: running total returned via Goods Return Note
        // (legacy retd_qty). Deliberately NOT decrementing IssuedQuantity the way
        // legacy IN_RTN1.PRG mutates issd_qty in place — IssuedQuantity stays a
        // monotonic "gross ever issued" audit total, same principle already applied
        // to Balance being derived rather than a second mutable total. Net balance
        // becomes OrderedQuantity - IssuedQuantity + ReturnedQuantity wherever it's
        // computed (e.g. Stock Movement Report), so a return still correctly frees
        // up balance without corrupting the gross-issued history.
        public decimal ReturnedQuantity { get; set; }

        // GTN traceability addition: running cumulative totals for Goods Transfer Note
        // activity (legacy in_stmst.trou_qty / trin_qty). Kept as two separate monotonic
        // "gross ever transferred" audit totals rather than folding straight into a single
        // mutable BalanceQuantity, same principle already applied to IssuedQuantity/
        // ReceivedQuantity/ReturnedQuantity above. Net balance wherever it's computed
        // (e.g. Stock Movement Report) becomes:
        //   OrderedQuantity - IssuedQuantity + ReturnedQuantity + TransferInQuantity - TransferOutQuantity
        public decimal TransferInQuantity { get; set; }
        public decimal TransferOutQuantity { get; set; }

        // SRN traceability addition: running total returned to supplier via Supplier
        // Return Note (legacy sret_qty). Same monotonic "gross ever returned" audit-total
        // principle as ReturnedQuantity/TransferIn/TransferOut above — net balance wherever
        // it's computed (e.g. Stock Movement Report) subtracts this the same way
        // TransferOutQuantity is subtracted.
        public decimal SupplierReturnQuantity { get; set; }

        // DGN traceability addition: running total damaged via Damaged Goods Note
        // (legacy damg_qty). Same monotonic "gross ever damaged" audit-total principle
        // as SupplierReturnQuantity/TransferIn/TransferOut above. NOTE: the Stock
        // Movement Report does NOT read this column — it already aggregates
        // OrderwiseStock.DamagedQuantity live across stores (see that entity), which is
        // the correct per-store source of truth. This master-level total exists purely
        // for 1:1 parity with legacy's in_stmst.damg_qty and quick per-item access
        // without a store join, same as every other note type's master total.
        public decimal DamagedQuantity { get; set; }

        // AIN traceability addition: running total issued via Additional Issue Note
        // (legacy in_stmst.issd_qty — shared with GIN in legacy, since legacy only ever
        // had one issd_qty column). Per explicit product decision (2026-08-09), this
        // system gives AIN its own dedicated monotonic column instead of commingling into
        // IssuedQuantity, matching the same "one column per note type" convention already
        // used for ReturnedQuantity/TransferIn/TransferOut/SupplierReturnQuantity/
        // DamagedQuantity above. Net balance wherever it's computed (e.g. Stock Movement
        // Report) subtracts this the same way IssuedQuantity is subtracted.
        public decimal AdditionalIssuedQuantity { get; set; }
    }
}
