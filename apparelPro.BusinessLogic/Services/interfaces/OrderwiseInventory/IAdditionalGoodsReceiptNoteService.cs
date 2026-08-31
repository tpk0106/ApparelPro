using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IAdditionalGoodsReceiptNoteService
    {
        // Validates the Buyer/Order exists, then returns every Additional Process
        // assignment (GarmentAdditionalCost) under it, with running Issued/Received
        // totals from OrderwiseStock - powers the item picker.
        Task<List<ArnReceivableStockRowServiceModel>> GetReceivableStockByBuyerOrderAsync(
            int buyerCode, string order);

        // Commits a complete Additional Goods Receipt Note atomically. Each line may
        // target a different Buyer/Order/Process (unlike AIN's single header-level
        // Buyer/Order) - validates every line independently, then increases physical
        // stock and revalues the order-level master price.
        Task<bool> CommitAdditionalGoodsReceiptNoteAsync(
            ArnHeaderServiceModel header,
            List<ArnLineItemServiceModel> lines,
            string username);

        // Modern equivalent of legacy IN_ARN2.PRG - loads an already-committed ARN by
        // its document number for printing.
        Task<ArnPrintDetailsServiceModel> GetArnPrintDetailsAsync(string arnNumber);
    }
}
