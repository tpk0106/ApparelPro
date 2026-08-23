using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPurchaseOrderService
    {
        Task<PaginationResult<PurchaseOrderServiceModel>> GetPurchaseOrderAsync(int pageNumber, int pageSize, 
            string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<PurchaseOrderServiceModel> GetPurchaseOrderByBuyerAndOrderAsync(int buyer, string ordr);
        Task<List<string>> GetPurchaseOrderByBuyerCodeAsync(int buyer);
        //Task<List<string>> GetTypesByBuyerCodeAndOrderAsync(int buyer, string order);
        //Task<List<StyleDetailsServiceModel>> GetStylesByBuyerCodeAndOrderCodeAndTypeCodeAsync(int buyer, string order, int type);
        Task<PurchaseOrderServiceModel> AddPurchaseOrderAsync(CreatePurchaseOrderServiceModel createCurrencyServiceModel);
        Task UpdatePurchaseOrderAsync(UpdatePurchaseOrderServiceModel updateCurrencyServiceModel);
        Task<bool> SaveSupplierPurchaseOrderAsync(
            string purchaseNumber, string supplierCode, string storeCode,
            string proformaNo, DateOnly? proformaDate, string currencyCode,
            List<SupplierPurchaseOrderDetails> lineItems);
        Task DeletePurchaseOrderAsync(string code);
        //Task SaveSupplierPurchaseOrderAsync(SaveSupplierPORequestServiceModel request);
        //// 1. Fetches style-scoped planned budget profiles where BalanceQuantity > 0
        //Task<List<AvailableBudgetLineServiceModel>> GetUnfulfilledBudgetLinesAsync(int buyerCode, string order);

        //// 2. Commits the fully compiled, multi-table supplier PO transaction
        //Task<bool> SaveSupplierPurchaseOrderAsync(SaveSupplierPORequestServiceModel request);

    }
}
