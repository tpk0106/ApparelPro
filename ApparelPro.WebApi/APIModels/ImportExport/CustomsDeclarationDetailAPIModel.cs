namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class CustomsDeclarationDetailAPIModel
    {
        public CustomsDeclarationHeaderAPIModel Header { get; set; } = null!;
        public List<CustomsDeclarationLineAPIModel> Lines { get; set; } = new();
        public List<CustomsDeclarationAttachedDocumentAPIModel> AttachedDocuments { get; set; } = new();
    }
}
