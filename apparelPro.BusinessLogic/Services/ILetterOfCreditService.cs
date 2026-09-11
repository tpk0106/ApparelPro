using apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ILetterOfCreditService
    {
        Task<LetterOfCreditDetailServiceModel?> GetByKeyAsync(string bankCode, string lcNo);
        Task<LetterOfCreditDetailServiceModel> SaveAsync(SaveLetterOfCreditServiceModel serviceModel);
        Task<bool> DeleteAsync(string bankCode, string lcNo);
    }
}
