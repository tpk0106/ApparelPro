namespace ApparelPro.Data.Models.Production
{
    public class StyleComponentBreakdown
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public int ComponentSequence { get; set; }
        public string ComponentCode { get; set; } = null!;
    }
}
