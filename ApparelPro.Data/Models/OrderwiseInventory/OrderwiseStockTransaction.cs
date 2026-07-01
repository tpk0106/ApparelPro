using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderwiseInventory
{
    public class OrderwiseStockTransaction
    {
        public int Id { get; set; } // Auto-increment Identity Seed Primary Key

        public string DocumentNumber { get; set; } = null!; // xdocno (SRN / GIN / GRN number)
        public string TransactionType { get; set; } = null!; // "0S" = SRN, "GI" = GIN, etc.
        public DateTime TransactionDate { get; set; } // xdate

        public int BuyerCode { get; set; } // xbuyer
        public string Order { get; set; } = null!; // xorder
        public string DepartmentCode { get; set; } = null!; // xdept

        // Line-Item Material Tracking Parameters
        public string StockCode { get; set; } = null!; // Splits from character positions
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }

        public string CreatedByUsername { get; set; } = null!;
    }
}
