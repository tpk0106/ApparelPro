using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService
{
    public class CreateOrderItemFeatureServiceModel
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;

        public string? Feature1Label { get; set; } // e.g. "TYPE" instead of "TY"
        public string? Feature2Label { get; set; } // e.g. "NO OF HOLES" instead of "NH"
        public string? Feature3Label { get; set; } // e.g. "SIZE" instead of "SZ"
        public string? Feature4Label { get; set; } // e.g. "COLOUR" instead of "CL"

        public decimal? CostPerUnit { get; set; }
    }
}
