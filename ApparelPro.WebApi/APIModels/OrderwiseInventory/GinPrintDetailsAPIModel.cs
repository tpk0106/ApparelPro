namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GinPrintDetailsAPIModel
    {
        public GinPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GinPrintLineAPIModel> Lines { get; set; } = new();
    }
}
