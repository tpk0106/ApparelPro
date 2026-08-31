namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ItemWiseStockBalanceHeaderServiceModel
    {
        public string FromRange { get; set; } = null!; // 6-char Stock+Item code
        public string ToRange { get; set; } = null!;
        public string Currency { get; set; } = "";
        public int TotalLineItems { get; set; }
        public decimal TotalReceivedValue { get; set; }
        public decimal TotalBalanceValue { get; set; }
    }
}
