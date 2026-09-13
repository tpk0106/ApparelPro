namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class SaveValueDeclarationAPIModel
    {
        public ValueDeclarationHeaderAPIModel Header { get; set; } = new();
        public List<ValueDeclarationLineAPIModel> Lines { get; set; } = new();
    }
}
