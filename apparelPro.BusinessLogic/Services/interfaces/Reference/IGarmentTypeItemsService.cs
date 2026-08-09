using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeItemsService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reference
{
    public interface IGarmentTypeItemsService
    {
        Task<List<GarmentTypeItemServiceModel>> GetGarmentTypeItemsAsync(int garmentTypeId);

        Task<GarmentTypeItemServiceModel> SaveGarmentTypeItemAsync(SaveGarmentTypeItemServiceModel request);

        Task<bool> DeleteGarmentTypeItemAsync(int garmentTypeId, string stockCode, string itemCode);
    }
}
