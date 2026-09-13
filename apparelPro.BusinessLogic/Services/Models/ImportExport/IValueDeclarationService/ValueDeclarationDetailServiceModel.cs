namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IValueDeclarationService
{
    public class ValueDeclarationDetailServiceModel
    {
        public ValueDeclarationHeaderServiceModel Header { get; set; } = new();
        public List<ValueDeclarationLineServiceModel> Lines { get; set; } = new();
    }
}
