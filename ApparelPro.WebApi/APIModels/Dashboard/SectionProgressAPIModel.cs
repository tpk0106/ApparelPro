namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class SectionProgressAPIModel
    {
        public string SectionCode { get; set; } = null!;
        public string SectionDescription { get; set; } = string.Empty;
        public decimal ToDateQuantity { get; set; }
        public decimal CeilingQuantity { get; set; }
    }
}
