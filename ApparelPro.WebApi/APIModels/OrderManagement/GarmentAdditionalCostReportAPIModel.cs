namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class GarmentAdditionalCostReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = "";
        public List<GarmentAdditionalCostCategoryAPIModel> Categories { get; set; } = new();
    }

    public class GarmentAdditionalCostCategoryAPIModel
    {
        public string AdditionalCostCode { get; set; } = "";
        public string AdditionalCostName { get; set; } = "";
        public List<GarmentAdditionalCostLineAPIModel> Lines { get; set; } = new();
        public decimal TotalValue { get; set; }
    }

    public class GarmentAdditionalCostLineAPIModel
    {
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public string StoreCode { get; set; } = "";
        public string StoreName { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Value { get; set; }
    }
}
