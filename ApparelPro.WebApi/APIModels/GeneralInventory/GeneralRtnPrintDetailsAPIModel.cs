namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralRtnPrintDetailsAPIModel
    {
        public GeneralRtnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralRtnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
