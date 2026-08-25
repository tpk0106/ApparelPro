namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    // Soft, override-able max-stock warning (plain Yes/No in legacy, no permission
    // gate) - distinct from MinStockOverrideRequiredException which is manager-only.
    public class MaxStockOverrideRequiredException : Exception
    {
        public MaxStockOverrideRequiredException(string message) : base(message) { }
    }
}
