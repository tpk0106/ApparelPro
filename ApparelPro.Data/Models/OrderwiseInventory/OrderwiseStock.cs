using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderwiseInventory
{
    // Replicates in_stock.dbf (Store-wise Raw Material Inventories)
    public class OrderwiseStock
    {
        public int Id { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }

        // 🚀 ADDED MISSING LEDGER COLUMNS FROM YOUR DATABASE SNAPSHOT:
        public decimal QtyInHand { get; set; }      // QTY_IN_HD
        public decimal ShadowBalance { get; set; }   // SHDW_BAL
        public decimal DamagedQuantity { get; set; } // DAMG_QTY
        public decimal ToDateIssued { get; set; }    // TO_DT_ISS
        public decimal ToDateReceived { get; set; }  // TO_DT_REC
        public DateTime? LastDateIssued { get; set; } // L_DT_ISS
        public DateTime? LastDateReceived { get; set; } // L_DT_REC
        public decimal StrnBalance { get; set; }      // SRN_BAL
    }
}
