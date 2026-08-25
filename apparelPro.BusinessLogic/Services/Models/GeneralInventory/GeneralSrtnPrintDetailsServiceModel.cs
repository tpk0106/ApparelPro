namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSrtnPrintDetailsServiceModel
    {
        public GeneralSrtnPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralSrtnPrintLineServiceModel> Lines { get; set; } = new();
    }
}
