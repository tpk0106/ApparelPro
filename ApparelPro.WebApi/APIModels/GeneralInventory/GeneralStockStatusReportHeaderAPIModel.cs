namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockStatusReportHeaderAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalLineItems { get; set; }
    }
}
