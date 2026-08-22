namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionProgressGraphAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public List<ProductionProgressPointAPIModel> EstimatedSeries { get; set; } = new();
        public List<ProductionProgressPointAPIModel> ActualSeries { get; set; } = new();
    }
}
