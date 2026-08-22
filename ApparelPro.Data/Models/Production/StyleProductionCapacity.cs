namespace ApparelPro.Data.Models.Production
{
    // PR_CAL - one row per style, holding the line-balancing calculator's
    // last computed target daily output. Upserted every time a style's
    // Operation Breakdown is saved (StyleOperationBreakdownService).
    public class StyleProductionCapacity
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public decimal OutputPerDay { get; set; }
    }
}
