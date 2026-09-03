namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class CurrentStyleAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string Source { get; set; } = null!;
    }
}
