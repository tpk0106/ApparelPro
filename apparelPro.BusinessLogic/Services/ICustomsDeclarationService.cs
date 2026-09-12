using apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsDeclarationService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICustomsDeclarationService
    {
        Task<CustomsDeclarationDetailServiceModel?> GetByCusNoAsync(string cusNo);
        Task<CustomsDeclarationDetailServiceModel> SaveAsync(SaveCustomsDeclarationServiceModel serviceModel);
        Task<bool> DeleteAsync(string cusNo);
    }
}
