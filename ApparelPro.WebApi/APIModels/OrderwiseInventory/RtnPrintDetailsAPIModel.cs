namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class RtnPrintDetailsAPIModel
    {
        public RtnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<RtnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
