namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class SanPrintDetailsServiceModel
    {
        public SanPrintHeaderServiceModel Header { get; set; } = null!;
        public List<SanPrintLineServiceModel> Lines { get; set; } = new();
    }
}
