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
    }
}
