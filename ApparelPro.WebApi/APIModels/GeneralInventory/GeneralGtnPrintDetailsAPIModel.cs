namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGtnPrintDetailsAPIModel
    {
        public GeneralGtnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralGtnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
