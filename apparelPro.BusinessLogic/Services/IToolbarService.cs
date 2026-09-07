using apparelPro.BusinessLogic.Services.Models.Toolbar.IToolbarService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IToolbarService
    {
        Task<ToolbarPreferenceServiceModel> GetPreferencesAsync(string userEmail);

        Task<ToolbarPreferenceServiceModel> SavePreferencesAsync(
            string userEmail, SaveToolbarPreferenceServiceModel saveToolbarPreferenceServiceModel);
    }
}
