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

        // 2. Commits the fully compiled, multi-table supplier PO transaction
        Task<bool> SaveSupplierPurchaseOrderAsync(SaveSupplierPORequestServiceModel request);
    }
}
