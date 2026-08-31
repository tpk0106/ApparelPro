namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class AinPrintDetailsAPIModel
    {
        public AinPrintHeaderAPIModel Header { get; set; } = null!;
        public List<AinPrintLineAPIModel> Lines { get; set; } = new();
    }
}
