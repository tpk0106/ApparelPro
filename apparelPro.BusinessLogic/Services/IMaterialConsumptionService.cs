using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.WebApi.APIModels.OrderManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services
{
    public interface IMaterialConsumptionService
    {
        // 1. Fetches all baseline items to populate the left selection grid
        Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync();

        // 2. Looks up the item mapping rules and swaps metadata keys for clear user labels
        Task<OrderItemFeatureServiceModel?> GetDynamicFeatureHeadersAsync(string stockCode, string itemCode);
        Task<decimal> ConvertUnitAsync(string fromUnit, string toUnit, decimal quantity);
        Task<decimal> CalculateMaterialConsumptionAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            string? garmentColor, string? garmentSize,
            string parentOrderUnit, string consumptionUnit, string finalItemUnit,
            decimal quantityPerGarment, decimal allowancePercentage);

        // New: Transactional Entry Commit Operation
        Task<bool> SaveMaterialConsumptionEntryAsync(CreateMaterialConsumptionEntryRequestServiceModel request);
        Task<List<StyleMaterialConsumptionLedger>> GetLedgerEntriesByStyleAsync(int buyerCode, string order, int typeCode, string styleCode);
        Task<bool> DeleteConsumptionEntryAsync(int buyerCode, string order, int typeCode, string styleCode, string stockCode, string itemCode, string color, string size);

        Task<List<OrderItemServiceModel>> GetAvailableMaterialsLookupAsync(int buyerCode, string order, int typeCode, string styleCode);

        //Task<StyleApprovalDetailsServiceModel?> GetStyleApprovalDetailsAsync(int buyerCode, string order, int typeCode, string styleCode);

        //  Task<StyleDimensionsLookupServiceModel> GetStyleDimensionsAsync(int buyerCode, string order, int typeCode, string styleCode);

    }
}
