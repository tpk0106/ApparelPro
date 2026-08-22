namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryStyleWiseRowAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string StyleCode { get; set; } = null!;
        public string? Description { get; set; }
        public decimal OrderQty { get; set; }
        public string Unit { get; set; } = null!;
        public List<ProductionSummaryStyleWiseSectionQtyAPIModel> SectionQuantities { get; set; } = new();
        public decimal UnitPrice { get; set; }
        public string? BasisCode { get; set; }
        public decimal Value { get; set; }
    }
}
