using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class RequisitionHeaderServiceModel
    {
        public string StrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
    }
}
