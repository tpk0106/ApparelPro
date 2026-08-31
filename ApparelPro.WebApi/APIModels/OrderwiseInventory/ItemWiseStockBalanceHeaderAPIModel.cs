namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ItemWiseStockBalanceHeaderAPIModel
    {
        public string FromRange { get; set; } = null!;
        public string ToRange { get; set; } = null!;
        public string Currency { get; set; } = "";
        public int TotalLineItems { get; set; }
        public decimal TotalReceivedValue { get; set; }
        public decimal TotalBalanceValue { get; set; }
    }
}
