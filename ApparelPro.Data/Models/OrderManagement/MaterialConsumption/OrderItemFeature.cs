using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderManagement.MaterialConsumption
{
    public class OrderItemFeature
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string? Feature1Type { get; set; } // Points to FeatureCode (e.g. 'TY', 'CL')
        public string? Feature2Type { get; set; }
        public string? Feature3Type { get; set; }
        public string? Feature4Type { get; set; }
        public decimal? CostPerUnit { get; set; }
    }
}
