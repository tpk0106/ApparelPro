namespace ApparelPro.WebApi.APIModels.Production
{
    public class DailyProductionEntryAPIModel
    {
        public string SectionCode { get; set; } = null!;
        public string SectionDescription { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal ToDateQuantity { get; set; }
    }
}
