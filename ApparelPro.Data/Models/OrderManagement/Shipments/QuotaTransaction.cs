using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderManagement.Shipments
{
    public class QuotaTransaction
    {
        public int Id { get; set; }
        public string Action { get; set; } = "BKD"; // action ('BKD')

        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string NewOrder { get; set; } = null!;

        public string QuotaStatus { get; set; } = null!;
        public string FromYearMonth { get; set; } = null!;
        public string ToYearMonth { get; set; } = null!;
        public string QuotaCountry { get; set; } = null!;
        public string QuotaCategory { get; set; } = null!;
        public string QuotaType { get; set; } = null!;

        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
