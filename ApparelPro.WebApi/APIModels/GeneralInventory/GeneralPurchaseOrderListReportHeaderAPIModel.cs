namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralPurchaseOrderListReportHeaderAPIModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public int TotalLineItems { get; set; }
    }
}
