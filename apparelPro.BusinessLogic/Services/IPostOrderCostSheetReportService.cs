using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPostOrderCostSheetReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPostOrderCostSheetReportService
    {
        Task<PostOrderCostSheetReportServiceModel> GetPostOrderCostSheetReportAsync(
            int buyerCode,
            string order,
            decimal percentOfTotalValue,
            decimal freightCharges,
            DateTime? actualShippedDate);
    }
}
