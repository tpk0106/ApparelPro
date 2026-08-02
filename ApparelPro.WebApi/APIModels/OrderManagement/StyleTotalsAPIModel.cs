namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class StyleTotalsAPIModel
    {
        public decimal TotalQuantity { get; set; }
        public string MainUnit { get; set; } = null!;
        public decimal OrderTotalQuantity { get; set; }
        public bool ExceedsOrderQuantity { get; set; }
    }
}
