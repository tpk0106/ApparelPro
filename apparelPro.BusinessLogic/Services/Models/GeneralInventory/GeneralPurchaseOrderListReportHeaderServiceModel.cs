namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralPurchaseOrderListReportHeaderServiceModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public int TotalLineItems { get; set; }
    }
}
