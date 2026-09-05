namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class RtnPrintDetailsServiceModel
    {
        public RtnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<RtnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
