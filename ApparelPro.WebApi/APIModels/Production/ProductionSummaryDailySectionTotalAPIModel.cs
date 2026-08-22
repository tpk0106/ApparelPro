namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryDailySectionTotalAPIModel
    {
        public string SectionCode { get; set; } = null!;
        public decimal ProQuantity { get; set; }
        public decimal ToDateQuantity { get; set; }
        public decimal Balance { get; set; }
    }
}
