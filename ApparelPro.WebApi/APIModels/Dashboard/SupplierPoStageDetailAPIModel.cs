namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class SupplierPoStageDetailAPIModel
    {
        public decimal CoveragePercent { get; set; }
        public decimal RaisedValue { get; set; }
        public decimal OutstandingValue { get; set; }
        public string Currency { get; set; } = "";
        public MaterialLineAPIModel? Bottleneck { get; set; }
        public List<MaterialLineAPIModel> OutstandingLines { get; set; } = new();
    }

    public class MaterialLineAPIModel
    {
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal RequiredQuantity { get; set; }
        public decimal RaisedQuantity { get; set; }
        public decimal OutstandingQuantity { get; set; }
        public decimal OutstandingValue { get; set; }
        public decimal CoveredPercent { get; set; }
    }
}
