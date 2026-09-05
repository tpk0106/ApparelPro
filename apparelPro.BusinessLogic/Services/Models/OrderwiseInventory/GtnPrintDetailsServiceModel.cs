namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GtnPrintDetailsServiceModel
    {
        public GtnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GtnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
