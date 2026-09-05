namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SanPrintDetailsAPIModel
    {
        public SanPrintHeaderAPIModel Header { get; set; } = null!;
        public List<SanPrintLineAPIModel> Lines { get; set; } = new();
    }
}
