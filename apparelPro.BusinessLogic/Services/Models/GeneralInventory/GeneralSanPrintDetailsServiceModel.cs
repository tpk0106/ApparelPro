namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSanPrintDetailsServiceModel
    {
        public GeneralSanPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralSanPrintLineServiceModel> Lines { get; set; } = new();
    }
}
