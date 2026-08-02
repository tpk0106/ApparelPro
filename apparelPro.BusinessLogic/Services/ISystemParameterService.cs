using apparelPro.BusinessLogic.Services.Models.SystemConfiguration.ISystemParameterService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ISystemParameterService
    {
        Task<IEnumerable<SystemParameterServiceModel>> GetAllParametersAsync();
        Task<string?> GetValueAsync(string parameterKey);
        Task<bool> GetBoolValueAsync(string parameterKey, bool defaultValue = false);
        Task UpdateParameterAsync(string parameterKey, string value);
    }
}
