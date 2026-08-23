namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class CreateColorQuantityRatioAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; }
        public int TypeCode { get; set; }
        public string StyleCode { get; set; }
        public string Color { get; set; }
        public string? Description { get; set; }
        public decimal Ratio { get; set; }
        public decimal Quantity { get; set; }
    }
}
