namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionAnalysisRowAPIModel
    {
        public DateOnly Date { get; set; }
        public string LineCode { get; set; } = null!;
        public List<ProductionAnalysisSectionQtyAPIModel> SectionQuantities { get; set; } = new();
        public decimal Total { get; set; }
    }
}
