namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ArnPrintDetailsServiceModel
    {
        public ArnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<ArnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
