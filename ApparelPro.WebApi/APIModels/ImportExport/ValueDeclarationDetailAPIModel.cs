namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class ValueDeclarationDetailAPIModel
    {
        public ValueDeclarationHeaderAPIModel Header { get; set; } = new();
        public List<ValueDeclarationLineAPIModel> Lines { get; set; } = new();
    }
}
