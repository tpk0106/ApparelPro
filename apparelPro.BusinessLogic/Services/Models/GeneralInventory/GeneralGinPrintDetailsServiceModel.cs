namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGinPrintDetailsServiceModel
    {
        public GeneralGinPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralGinPrintLineServiceModel> Lines { get; set; } = new();
    }
}
