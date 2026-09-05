namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class DgnPrintDetailsServiceModel
    {
        public DgnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<DgnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
