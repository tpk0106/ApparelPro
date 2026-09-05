namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class SrnPrintDetailsServiceModel
    {
        public SrnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<SrnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
