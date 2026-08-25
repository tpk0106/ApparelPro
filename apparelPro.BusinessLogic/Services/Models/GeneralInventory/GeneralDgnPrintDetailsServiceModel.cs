namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralDgnPrintDetailsServiceModel
    {
        public GeneralDgnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralDgnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
