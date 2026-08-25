namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralPoCommitResultServiceModel
    {
        public string PoNumber { get; set; } = null!;

        // Non-blocking advisories, mirroring legacy's "Continue...?" warning dialogs
        // (re-order level / max stock / re-order quantity) - informational only, never
        // rejected server-side, since legacy itself lets the user proceed past every one.
        public List<string> Warnings { get; set; } = new();
    }
}
