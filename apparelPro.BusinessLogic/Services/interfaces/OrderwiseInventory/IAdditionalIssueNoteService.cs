using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IAdditionalIssueNoteService
    {
        // Validates the Buyer/Order exists, then returns every stock row under it that
        // also has a Material Consumption profile - powers the item picker.
        Task<List<AinIssuableStockRowServiceModel>> GetIssuableStockByBuyerOrderAsync(
            int buyerCode, string order);

        // Commits a complete Additional Issue Note atomically: validates Sub Contractor,
        // Additional Process, and the order-level allocation headroom for every line, then
        // performs the physical issue directly (no separate STRN/GIN two-step - AIN is a
        // one-shot direct issue in legacy IN_AIN1.PRG).
        Task<bool> CommitAdditionalIssueNoteAsync(
            AinHeaderServiceModel header,
            List<AinLineItemServiceModel> lines,
            string username);

        // Modern equivalent of legacy IN_AIN2.PRG - loads an already-committed AIN by
        // its document number for printing.
        Task<AinPrintDetailsServiceModel> GetAinPrintDetailsAsync(string ainNumber);
    }
}
