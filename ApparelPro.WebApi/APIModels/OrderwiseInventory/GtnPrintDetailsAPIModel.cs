namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GtnPrintDetailsAPIModel
    {
        public GtnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GtnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
