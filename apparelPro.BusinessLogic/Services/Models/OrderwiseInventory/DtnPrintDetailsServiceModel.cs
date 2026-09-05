namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class DtnPrintDetailsServiceModel
    {
        public DtnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<DtnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
