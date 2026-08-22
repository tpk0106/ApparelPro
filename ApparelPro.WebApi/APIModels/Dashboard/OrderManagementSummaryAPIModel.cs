namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class OrderManagementSummaryAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public decimal? OrderQuantity { get; set; }
        public string? Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public DateOnly OrderDate { get; set; }
        public DateOnly? EstimateApprovalDate { get; set; }
        public decimal ShippedQuantity { get; set; }
        public List<ColorSizeMixAPIModel> ColorSizeMix { get; set; } = new();
    }
}
