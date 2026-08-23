namespace ApparelPro.Data.Models.OrderManagement
{
    // Legacy od_clqr.dbf equivalent - the colour-level ratio/quantity allocation
    // row (one per Buyer/Order/Type/Style/Color), entered and saved BEFORE the
    // size-level breakdown (ColorSizeDetails/od_cszdt). Previously this stage
    // was never persisted as its own table - only derived after the fact by
    // summing ColorSizeDetails.Qty per colour.
    public class ColorQuantityRatio
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; }
        public int TypeCode { get; set; }
        public string StyleCode { get; set; }
        public string Color { get; set; }
        public string? Description { get; set; }
        public decimal Ratio { get; set; }
        public decimal Quantity { get; set; }
    }
}
