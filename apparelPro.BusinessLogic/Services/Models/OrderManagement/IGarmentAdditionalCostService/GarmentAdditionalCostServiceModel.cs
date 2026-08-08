namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IGarmentAdditionalCostService
{
    public class GarmentAdditionalCostServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string AdditionalCostCode { get; set; } = null!;
        public string AdditionalCostName { get; set; } = null!;
        public string Description { get; set; } = "";
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Feature1 { get; set; } = "";
        public string Feature2 { get; set; } = "";
        public string Feature3 { get; set; } = "";
        public string Feature4 { get; set; } = "";
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public string StoreCode { get; set; } = null!;
        public string StoreName { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public bool IsCostPerGarment { get; set; }
        public bool IsSemiFinishedGarment { get; set; }
    }
}
