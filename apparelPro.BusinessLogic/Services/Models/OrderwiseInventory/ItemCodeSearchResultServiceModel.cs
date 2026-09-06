namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ItemCodeSearchResultServiceModel
    {
        public string Code { get; set; } = null!; // 6-char Stock+Item composite code
        public string Description { get; set; } = "";
    }
}
