using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService
{
    public class StyleApprovalDetailsServiceModel
    {        
        public DateOnly? EstimateApprovalDate { get; set; }
        public string? EstimateApprovalUserName { get; set; }
    }
}
