using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderManagement.MaterialConsumption
{
    // Replicates od_sacc2.dbf (Material Cost Profiles)
    public class StyleMaterialCostProfile
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;

        // Collapsed 2026-07-22: this used to be split into StockCode(2) + ItemCode(4) +
        // Feature1-4(4 each) — a normalization the legacy od_sacc2.dbf never actually had
        // (its ITEM_CD field is a single 22-char blob, confirmed by reading the DBF header
        // directly: no StockCode/Feature columns exist there at all). Every other table that
        // shares this same 22-char convention (OrderwiseStockMaster, OrderwiseStockTransaction,
        // PODetails) already stores it as one field, so this entity now matches them — no more
        // packing/unpacking seam at the PO budget lookup or STRN/Stock Movement report joins.
        // ItemCode is the full 22-char composite: StockCode(2) + ItemCode(4) + Feature1-4(4 each).
        public string ItemCode { get; set; } = null!;

        public string Description { get; set; } = null!;
        public string ItemUnit { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal BalanceQuantity { get; set; }

        // Legacy od_sacc2 fields that were missing from this replica:
        // SUPP_CD - preferred supplier for this planned material line.
        public string SupplierCode { get; set; } = "";
        // TOT_CON - the original/cumulative planned total. BalanceQuantity
        // (bal_qty) is a rolling figure that goes up when consumption entries
        // are added/edited and down when POs are raised against it; this
        // field only ever tracks the former, so the original planned amount
        // survives even after POs have drawn the balance down to 0.
        public decimal TotalConsumption { get; set; }
    }
}
