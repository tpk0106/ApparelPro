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
        public decimal ReceivedQuantity { get; set; }
    }
}
