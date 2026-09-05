namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DgnPrintDetailsAPIModel
    {
        public DgnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<DgnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
