namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class GarmentAdditionalCostAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public string AdditionalCostCode { get; set; } = "";
        public string AdditionalCostName { get; set; } = "";
        public string Description { get; set; } = "";
        public string StockCode { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Feature1 { get; set; } = "";
        public string Feature2 { get; set; } = "";
        public string Feature3 { get; set; } = "";
        public string Feature4 { get; set; } = "";
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public string StoreCode { get; set; } = "";
        public string StoreName { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public bool IsCostPerGarment { get; set; }
        public bool IsSemiFinishedGarment { get; set; }
    }
}
