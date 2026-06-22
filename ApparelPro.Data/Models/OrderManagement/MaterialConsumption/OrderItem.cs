using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderManagement.MaterialConsumption
{
    public class OrderItem
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
