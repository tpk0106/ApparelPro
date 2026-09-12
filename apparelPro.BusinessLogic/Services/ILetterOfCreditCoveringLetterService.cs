using apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditCoveringLetterService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ILetterOfCreditCoveringLetterService
    {
        Task<LetterOfCreditCoveringLetterServiceModel?> GetByKeyAsync(string bankCode, string lcNo);
        Task<LetterOfCreditCoveringLetterServiceModel> SaveAsync(LetterOfCreditCoveringLetterServiceModel serviceModel);
        Task<bool> DeleteAsync(string bankCode, string lcNo);
    }
}
