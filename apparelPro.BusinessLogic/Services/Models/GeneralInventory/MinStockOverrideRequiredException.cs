namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    // Distinguishes a soft, override-able min-stock warning from a hard
    // InvalidOperationException block, so the controller can return a distinct
    // "needs override" response instead of a flat error - same pattern as
    // Orderwise's ExactConsumptionOverrideRequiredException.
    public class MinStockOverrideRequiredException : Exception
    {
        public MinStockOverrideRequiredException(string message) : base(message) { }
    }
}
