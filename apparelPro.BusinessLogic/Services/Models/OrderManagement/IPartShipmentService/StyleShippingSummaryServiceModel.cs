using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPartShipmentService
{
    public class StyleShippingSummaryServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal TotalContractQuantity { get; set; }
        public decimal TotalScheduledQuantity { get; set; }
        public decimal RemainingUnscheduledBalance => TotalContractQuantity - TotalScheduledQuantity;
    }
}
