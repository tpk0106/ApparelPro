namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class OrderGtnPrintDetailsServiceModel
    {
        public OrderGtnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<OrderGtnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
