namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryStyleWiseDetailedLineAPIModel
    {
        public string LineCode { get; set; } = null!;
        public decimal OrderQty { get; set; }
        public List<ProductionSummaryStyleWiseSectionQtyAPIModel> SectionQuantities { get; set; } = new();
    }
}
