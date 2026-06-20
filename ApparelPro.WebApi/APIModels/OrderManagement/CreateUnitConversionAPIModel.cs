namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class CreateUnitConversionAPIModel
    {
        public string FromUnit { get; set; }
        public string ToUnit { get; set; }
        public decimal? Measure { get; set; }
    }
}
