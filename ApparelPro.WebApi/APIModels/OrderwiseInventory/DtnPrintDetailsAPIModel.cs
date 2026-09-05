namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DtnPrintDetailsAPIModel
    {
        public DtnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<DtnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
