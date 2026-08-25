namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGtnPrintDetailsServiceModel
    {
        public GeneralGtnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralGtnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
