using ApparelPro.WebApi.APIModels.OrderManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services
{
    public interface IGarmentTypeItemsService
    { 
      // 1. FIXED SIGNATURE: Streams lines using the real primary key integer GarmentTypeId!
        Task<List<GarmentTypeItems>> GetBreakdownLinesByGarmentTypeAsync(int garmentTypeId);

        // 2. FIXED SIGNATURE: Uses integer parent ID for relational type safety locks
        Task<bool> SaveBreakdownLineAsync(int garmentTypeId, string stockCode, string itemCode, string unit, decimal quantity);

        // 3. FIXED SIGNATURE: Uses integer parent ID for targeted row purges
        Task<bool> DeleteBreakdownLineAsync(int garmentTypeId, string stockCode, string itemCode);

    }
}
