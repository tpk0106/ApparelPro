using ApparelPro.Data.DomainEvents;

namespace ApparelPro.Data.Models.OrderwiseInventory
{
    // Raised once per distinct (Buyer, Order, Type, Style) touched by a GRN
    // posting - a single GRN can receive items against several styles at
    // once, and each one needs its own completion check.
    public record GoodsReceivedEvent(int Buyer, string Order, int Type, string Style) : IDomainEvent;
}
