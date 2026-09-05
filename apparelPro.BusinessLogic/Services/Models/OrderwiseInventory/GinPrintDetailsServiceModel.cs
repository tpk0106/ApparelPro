namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GinPrintDetailsServiceModel
    {
        public GinPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GinPrintLineServiceModel> Lines { get; set; } = new();
    }
}
