using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockItemAvailabilityDetails
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal PhysicalQtyInHand { get; set; }
        public decimal ShadowAllocatedBalance { get; set; }
        public decimal RequisitionedSrnBalance { get; set; }
        public decimal NetAvailableBalance => PhysicalQtyInHand - ShadowAllocatedBalance - RequisitionedSrnBalance;
    }
}
