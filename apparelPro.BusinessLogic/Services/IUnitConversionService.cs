using apparelPro.BusinessLogic.Services.Models.Reference.IUnitConversionService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IUnitConversionService
    {
        Task<PaginationResult<UnitConversionServiceModel>> GetUnitConversionsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);

        // Task<IEnumerable<UnitConversionServiceModel>> GetUnitsAsync();
        Task<UnitConversionServiceModel> GetUnitConversionByFromUnitAndToUnitAsync(string fromUnit, string toUnit);
        Task<UnitConversionServiceModel> AddUnitAsync(CreateUnitConversionServiceModel createUnitConversionServiceModel);
        Task UpdateUnitAsync(UpdateUnitConversionServiceModel updateUnitConversionServiceModel);
        Task DeleteUnitConversionAsync(string fromUnit, string toUnit);
        //Task<bool> DoesUnitConversionExistAsync(string code);

        Task<decimal> ConvertUnitAsync(string fromUnit, string toUnit, decimal quantity);
    }
}
