using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services
{
    public interface IStyleApprovalService
    {  // Executes the executive milestone lockout transaction footprint
        Task<bool> ApproveStyleEventsAsync(int buyerCode, string order, int typeCode, string styleCode, string approvedByUserId, DateTime approvalDate);
        Task<StyleApprovalDetailsServiceModel?> GetStyleApprovalDetailsAsync(int buyerCode, string order, int typeCode, string styleCode);

    }
}
