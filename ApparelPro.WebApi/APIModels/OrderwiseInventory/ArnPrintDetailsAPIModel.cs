namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ArnPrintDetailsAPIModel
    {
        public ArnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<ArnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
