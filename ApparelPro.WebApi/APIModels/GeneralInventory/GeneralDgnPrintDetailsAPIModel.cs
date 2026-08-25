namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralDgnPrintDetailsAPIModel
    {
        public GeneralDgnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralDgnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
