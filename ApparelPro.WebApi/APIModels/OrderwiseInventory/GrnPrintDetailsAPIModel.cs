namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnPrintDetailsAPIModel
    {
        public GrnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GrnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
