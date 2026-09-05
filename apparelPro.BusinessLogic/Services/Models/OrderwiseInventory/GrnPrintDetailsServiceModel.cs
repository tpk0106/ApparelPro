namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GrnPrintDetailsServiceModel
    {
        public GrnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GrnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
