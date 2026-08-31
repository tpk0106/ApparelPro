namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockReorderReportHeaderAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public int TotalLineItems { get; set; }
    }
}
