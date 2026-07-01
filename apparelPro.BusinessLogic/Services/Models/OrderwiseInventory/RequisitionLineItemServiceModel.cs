using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class RequisitionLineItemServiceModel
    {
        public string StockCode { get; set; } = null!;   // Material Category Prefix (e.g., "02")
        public string ItemCode { get; set; } = null!;    // Item Reference ID (e.g., "02BT")
        public string StoreCode { get; set; } = null!;   // The "Basis" / Store code (e.g., "STR")
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
