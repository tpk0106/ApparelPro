namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSrtnPrintDetailsAPIModel
    {
        public GeneralSrtnPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralSrtnPrintLineAPIModel> Lines { get; set; } = new();
    }
}
