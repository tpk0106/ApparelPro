namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStrnPrintDetailsAPIModel
    {
        public GeneralStrnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralStrnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
