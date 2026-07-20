using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services
{
    public interface ISupplierPurchaseOrderService
    {
        // 1. Fetches style-scoped planned budget profiles where BalanceQuantity > 0
        Task<List<AvailableBudgetLineServiceModel>> GetUnfulfilledBudgetLinesAsync(int buyerCode, string order);

        // 2. Commits the fully compiled, multi-table supplier PO transaction.
        // Returns the confirmed P/O number - for a new P/O this is the number
        // allocated server-side (the caller's PurchaseNumber is ignored), for
        // an edit it echoes back the same number that was passed in.
        Task<string> SaveSupplierPurchaseOrderAsync(SaveSupplierPORequestServiceModel request);
    }
}
