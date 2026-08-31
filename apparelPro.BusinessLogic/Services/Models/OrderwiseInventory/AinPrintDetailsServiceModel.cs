namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class AinPrintDetailsServiceModel
    {
        public AinPrintHeaderServiceModel Header { get; set; } = null!;
        public List<AinPrintLineServiceModel> Lines { get; set; } = new();
    }
}
