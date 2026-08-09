namespace ApparelPro.WebApi.APIModels.Reference
{
    public class SaveGarmentTypeItemAPIModel
    {
        public int GarmentTypeId { get; set; }
        public string StockCode { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
    }
}
