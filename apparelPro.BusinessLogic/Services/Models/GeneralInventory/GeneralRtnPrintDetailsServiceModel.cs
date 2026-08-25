namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralRtnPrintDetailsServiceModel
    {
        public GeneralRtnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralRtnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
