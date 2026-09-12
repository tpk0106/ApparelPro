namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class SaveCustomsDeclarationAPIModel
    {
        public CustomsDeclarationHeaderAPIModel Header { get; set; } = null!;
        public List<CustomsDeclarationLineAPIModel> Lines { get; set; } = new();
        public List<CustomsDeclarationAttachedDocumentAPIModel> AttachedDocuments { get; set; } = new();
    }
}
