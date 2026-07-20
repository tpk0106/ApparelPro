using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class OrderwiseStockLookupRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // The Basis code
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
    }
}
