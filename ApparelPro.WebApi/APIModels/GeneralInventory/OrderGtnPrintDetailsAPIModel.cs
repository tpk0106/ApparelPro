namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class OrderGtnPrintDetailsAPIModel
    {
        public OrderGtnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<OrderGtnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
