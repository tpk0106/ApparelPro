namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.ISubContractService
{
    public class SaveSubContractResultServiceModel
    {
        public SubContractServiceModel SubContract { get; set; } = null!;

        // Non-null = a non-blocking, informational warning that the sum of SubQuantity
        // across every Sub Contract row for this style now exceeds the style's ordered
        // quantity (References.Style.Quantity). Per explicit user decision (2026-08-09):
        // this NEVER blocks the save - it is advisory only, same spirit as
        // SupplierReturnNoteLinesGrid's "already reserved" warning on the frontend.
        public string? QuantityWarning { get; set; }
    }
}
