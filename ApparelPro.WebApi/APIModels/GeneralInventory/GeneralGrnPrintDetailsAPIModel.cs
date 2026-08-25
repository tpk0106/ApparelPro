namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGrnPrintDetailsAPIModel
    {
        public GeneralGrnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralGrnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
