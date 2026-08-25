namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    // "GeneralToOrder" = stock leaves a General store and lands under a Buyer/Order.
    // "OrderToGeneral" = the reverse. Modeled as an explicit string instead of legacy's
    // cryptic m_stat 1/2 "To"/"From" menu prompt.
    public static class OrderGtnDirection
    {
        public const string GeneralToOrder = "GeneralToOrder";
        public const string OrderToGeneral = "OrderToGeneral";
    }

    public class OrderGtnHeaderServiceModel
    {
        public string OgtnNumber { get; set; } = null!; // allocated by the C# backend (NoteType "OGTN")
        public string Direction { get; set; } = null!;  // OrderGtnDirection.GeneralToOrder / OrderToGeneral
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
    }
}
