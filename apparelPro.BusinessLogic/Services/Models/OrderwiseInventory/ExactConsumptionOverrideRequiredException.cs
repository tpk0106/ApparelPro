namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // Distinguishes a soft, override-able business rule violation from a hard
    // InvalidOperationException block, so the controller can catch it specifically
    // and return a distinct "needs override" response instead of a flat error.
    public class ExactConsumptionOverrideRequiredException : Exception
    {
        public ExactConsumptionOverrideRequiredException(string message) : base(message) { }
    }
}
