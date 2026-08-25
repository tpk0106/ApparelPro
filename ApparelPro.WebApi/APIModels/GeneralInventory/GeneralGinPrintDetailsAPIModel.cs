namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGinPrintDetailsAPIModel
    {
        public GeneralGinPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralGinPrintLineAPIModel> Lines { get; set; } = new();
    }
}
