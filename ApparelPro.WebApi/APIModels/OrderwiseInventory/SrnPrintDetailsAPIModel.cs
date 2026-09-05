namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SrnPrintDetailsAPIModel
    {
        public SrnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<SrnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
