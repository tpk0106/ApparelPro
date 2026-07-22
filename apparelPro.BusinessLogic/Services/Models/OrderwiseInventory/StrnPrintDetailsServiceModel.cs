namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StrnPrintDetailsServiceModel
    {
        public StrnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<StrnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
