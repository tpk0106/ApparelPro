using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStylewiseEvents;

namespace apparelPro.BusinessLogic.Services
{
    public interface IStylewiseEventService
    {// 1. Fetches or auto-initializes the milestone checklist for a specific style scope
        Task<List<StylewiseEventServiceModel>> GetStyleEventsAsync(int buyerCode, string order, int typeCode, string styleCode);

        // 2. Processes inline edits and saves updates to the ledger matrix
        Task<bool> UpdateStyleEventLineAsync(int buyerCode, string order, int typeCode, string styleCode, string eventCode, DateTime? scheduledDate, DateTime? actualDate, string? remarks);

        // 3. Extends an action command button block to manually append custom milestone lines
        Task<bool> AddCustomStyleEventLineAsync(int buyerCode, string order, int typeCode, string styleCode, string eventCode, DateTime? scheduledDate, DateTime? actualDate, string? remarks);

        // 4. Handles milestone deletion purges with structural security controls
        Task<bool> DeleteStyleEventLineAsync(int buyerCode, string order, int typeCode, string styleCode, string eventCode);

    }
}
