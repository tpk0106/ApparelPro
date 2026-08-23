namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    // Body for setting Style.ColorRatio / Style.SizeRatio - "R" (Ratio) or
    // "Q" (Quantity), matching legacy od_style's c_rt/s_rt flags.
    public class SetRatioModeAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; }
        public int TypeCode { get; set; }
        public string StyleCode { get; set; }
        public string Mode { get; set; } = "Q";
    }
}
