using apparelPro.BusinessLogic.Services.Models.ImportExport.IPackingListService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPackingListService
    {
        Task<PackingListDetailServiceModel> GetByLineKeyAsync(
            string invoiceNumber, int buyerCode, string order, int typeCode, string styleCode, string newOrder);
        Task<PackingListDetailServiceModel> SaveAsync(SavePackingListServiceModel serviceModel);
    }
}
