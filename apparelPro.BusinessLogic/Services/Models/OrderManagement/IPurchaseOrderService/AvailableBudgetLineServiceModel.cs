using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService
{
    public class AvailableBudgetLineServiceModel
    {
        // Holds the unified 22+ character composite string key (StockCode + ItemCode + Features)
        public string ItemCode { get; set; } = null!;
        public string ItemUnit { get; set; } = null!;
        public decimal BalanceQuantity { get; set; }
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
