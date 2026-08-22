namespace ApparelPro.WebApi.APIModels.Production
{
    public class CreateDailyProductionEntryAPIModel
    {
        public string SectionCode { get; set; } = null!;
        public decimal Hours { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
