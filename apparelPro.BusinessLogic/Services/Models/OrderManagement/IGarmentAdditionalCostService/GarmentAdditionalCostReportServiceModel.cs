namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IGarmentAdditionalCostService
{
    public class GarmentAdditionalCostReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public List<GarmentAdditionalCostCategoryServiceModel> Categories { get; set; } = new();
    }

    public class GarmentAdditionalCostCategoryServiceModel
    {
        public string AdditionalCostCode { get; set; } = null!;
        public string AdditionalCostName { get; set; } = null!;
        public List<GarmentAdditionalCostLineServiceModel> Lines { get; set; } = new();
        public decimal TotalValue { get; set; }
    }

    public class GarmentAdditionalCostLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string StoreCode { get; set; } = null!;
        public string StoreName { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Value { get; set; }
    }
}
