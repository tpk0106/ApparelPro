using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IUnitServiceT<T> where T : class
    {
        Task<IEnumerable<T>> GetUnitsAsync();
        Task<T> GetUnitByCodeAsync(string code);
        Task<T> AddUnitAsync(CreateUnitServiceModel createUnitServiceModel);
        Task UpdateUnitAsync(UpdateUnitServiceModel updateUnitServiceModel);
        Task DeleteUnitAsync(string code);
    }
}
