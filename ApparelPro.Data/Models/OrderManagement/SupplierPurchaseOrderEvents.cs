using ApparelPro.Data.DomainEvents;

namespace ApparelPro.Data.Models.OrderManagement
{
    public record SupplierPurchaseOrderRaisedEvent(int Buyer, string Order, int Type, string Style) : IDomainEvent;
}
