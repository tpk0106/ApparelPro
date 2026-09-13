using apparelPro.BusinessLogic.Services.Models.ImportExport.IBoatNoteService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IBoatNoteService
    {
        Task<BoatNoteDetailServiceModel?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<BoatNoteDetailServiceModel> SaveAsync(SaveBoatNoteServiceModel serviceModel);
        Task<bool> DeleteAsync(string invoiceNumber);
        Task<BoatNotePrintDetailsServiceModel?> GetPrintDetailsAsync(string invoiceNumber);
    }
}
