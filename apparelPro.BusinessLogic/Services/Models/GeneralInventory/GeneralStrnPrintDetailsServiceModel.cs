namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStrnPrintDetailsServiceModel
    {
        public GeneralStrnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralStrnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
