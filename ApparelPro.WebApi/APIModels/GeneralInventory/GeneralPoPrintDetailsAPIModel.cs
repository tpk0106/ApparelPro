namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralPoPrintDetailsAPIModel
    {
        public GeneralPoPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralPoPrintLineAPIModel> Lines { get; set; } = new();
    }
}
