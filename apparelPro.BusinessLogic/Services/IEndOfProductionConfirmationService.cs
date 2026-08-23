using apparelPro.BusinessLogic.Services.Models.Production.IEndOfProductionConfirmationService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IEndOfProductionConfirmationService
    {
        Task<EndOfProductionStatusServiceModel> GetStatusAsync(int buyerCode, string order, int typeCode, string styleCode);

        Task<EndOfProductionStatusServiceModel> ConfirmAsync(
            int buyerCode, string order, int typeCode, string styleCode, DateOnly endDate);
    }
}
