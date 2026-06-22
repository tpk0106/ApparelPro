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
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Feature1 { get; set; } = null!;
        public string Feature2 { get; set; } = null!;
        public string Feature3 { get; set; } = null!;
        public string Feature4 { get; set; } = null!;

        public string Description { get; set; } = null!;
        public string ItemUnit { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal BalanceQuantity { get; set; }
    }
}
