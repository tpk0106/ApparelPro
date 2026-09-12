namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsDeclarationService
{
    public class SaveCustomsDeclarationServiceModel
    {
        public CustomsDeclarationHeaderServiceModel Header { get; set; } = null!;
        public List<CustomsDeclarationLineServiceModel> Lines { get; set; } = new();
        public List<CustomsDeclarationAttachedDocumentServiceModel> AttachedDocuments { get; set; } = new();
    }
}
