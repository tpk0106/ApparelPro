namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StrnPrintDetailsAPIModel
    {
        public StrnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<StrnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
