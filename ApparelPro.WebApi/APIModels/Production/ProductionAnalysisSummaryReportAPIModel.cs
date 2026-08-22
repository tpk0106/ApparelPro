namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionAnalysisSummaryReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;

        public List<string> SectionCodes { get; set; } = new();
        public List<string> SectionDescriptions { get; set; } = new();
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;

        public List<ProductionAnalysisRowAPIModel> Rows { get; set; } = new();
        public List<ProductionAnalysisSectionQtyAPIModel> SectionTotals { get; set; } = new();

        public decimal AverageProductionQuantityOnFinalOutput { get; set; }
        public int FinalOutputProductionDays { get; set; }
        public int TotalDaysTakenForProduction { get; set; }
    }
}
