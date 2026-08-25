namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGrnPrintDetailsServiceModel
    {
        public GeneralGrnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralGrnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
