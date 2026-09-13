namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IValueDeclarationService
{
    public class SaveValueDeclarationServiceModel
    {
        public ValueDeclarationHeaderServiceModel Header { get; set; } = new();
        public List<ValueDeclarationLineServiceModel> Lines { get; set; } = new();
    }
}
