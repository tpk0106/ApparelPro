
namespace apparelPro.BusinessLogic.Services.interfaces.ISharedService
{
    public interface ISharedService
    {
        Task<string> GenerateNextDocumentNumberAsync(string noteType);
        Task<decimal> ConvertUnitAsync(string fromUnit, string toUnit, decimal quantity);
    }
}
