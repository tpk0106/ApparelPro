using ApparelPro.Data.DomainEvents;

namespace ApparelPro.Data.Models.Production
{
    // Raised whenever a Daily Production Entry is bulk-saved for a style -
    // may or may not be the entry that finally meets the target; the handler
    // decides that.
    public record ProductionRecordedEvent(int Buyer, string Order, int Type, string Style) : IDomainEvent;
}
