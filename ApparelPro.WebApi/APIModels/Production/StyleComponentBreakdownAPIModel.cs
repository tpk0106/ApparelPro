namespace ApparelPro.WebApi.APIModels.Production
{
    public class StyleComponentBreakdownAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public int ComponentSequence { get; set; }
        public string ComponentCode { get; set; } = null!;
    }
}
