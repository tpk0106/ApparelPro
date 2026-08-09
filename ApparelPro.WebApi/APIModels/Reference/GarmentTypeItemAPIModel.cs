namespace ApparelPro.WebApi.APIModels.Reference
{
    public class GarmentTypeItemAPIModel
    {
        public int Id { get; set; }
        public int GarmentTypeId { get; set; }
        public string GarmentTypeName { get; set; } = "";
        public string StockCode { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string ItemDescription { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
    }
}
