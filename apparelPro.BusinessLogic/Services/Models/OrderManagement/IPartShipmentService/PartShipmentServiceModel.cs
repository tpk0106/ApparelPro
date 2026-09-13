using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPartShipmentService
{
    public class PartShipmentServiceModel
    {
        public int Id { get; set; } // 0 for New Records, >0 for Inline Edits
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;

        public string NewOrder { get; set; } = null!;
        public string DestinationCode { get; set; } = null!;
        public DateTime ShipDate { get; set; }

        public string SubContractFlag { get; set; } = "N";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string ShippingMode { get; set; } = null!;
    }
}
