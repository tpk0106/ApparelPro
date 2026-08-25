namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralPoPrintDetailsServiceModel
    {
        public GeneralPoPrintHeaderServiceModel Header { get; set; } = null!;
        public List<GeneralPoPrintLineServiceModel> Lines { get; set; } = new();
    }
}
