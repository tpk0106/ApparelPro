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
    }
}
