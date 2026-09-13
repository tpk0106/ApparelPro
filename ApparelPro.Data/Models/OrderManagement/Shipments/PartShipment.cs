using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderManagement.Shipments
{
    public class PartShipment
    {
        public int Id { get; set; } // Internal database primary auto-increment identity seed

        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;

        public string NewOrder { get; set; } = null!; // new_order (Split Shipping Order Ref)
        public string DestinationCode { get; set; } = null!; // dest
        public DateTime ShipDate { get; set; } // ship_date

        public string SubContractFlag { get; set; } = "N"; // subcont ('Y'/'N')
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string ShippingMode { get; set; } = null!; // shp_mode ('SEA'/'AIR')

        public DateTime OrderDate { get; set; } // odate
        public decimal Balance { get; set; } // balance (Open delivery balance remaining)

    }
}
