using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPartShipmentService;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPartShipmentService
    { // 1. Fetches the upper grid context showing style metrics and scheduled cumulative summaries
        Task<StyleShippingSummaryServiceModel> GetStyleShippingSummaryAsync(int buyerCode, string order, int typeCode, string styleCode);

        // 2. Fetches the lower grid manifest tracking rows recorded for this specific style layout scope
        Task<List<PartShipment>> GetPartShipmentsByStyleAsync(int buyerCode, string order, int typeCode, string styleCode);

        // 3. Atomically saves or updates a partial delivery, coordinating quad-option quota balances and style rollups
        Task<bool> SavePartShipmentLineAsync(PartShipmentServiceModel request);

        // 4. Safely drops a partial shipment manifest line, fully refunding allocated quotas to reference tables
        Task<bool> DeletePartShipmentLineAsync(int id);

        // 5. All of a buyer's open (undelivered) part shipment balances, across every order/style -
        // backs the Commercial Invoice line picker (legacy ie_coin1's od_part selection list).
        Task<List<PartShipment>> GetOpenPartShipmentsByBuyerAsync(int buyerCode);
    }
}
