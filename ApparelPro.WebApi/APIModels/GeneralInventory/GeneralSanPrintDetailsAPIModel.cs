namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSanPrintDetailsAPIModel
    {
        public GeneralSanPrintHeaderAPIModel Header { get; set; } = null!;
        public List<GeneralSanPrintLineAPIModel> Lines { get; set; } = new();
    }
}
