using ApparelPro.Data.DomainEvents;

namespace ApparelPro.Data.Models.OrderManagement.Shipments
{
    // Raised whenever a Part Shipment line is saved (create or edit) for a
    // style - may or may not be the line that finally meets the full order
    // quantity; the handler decides that. Not raised from a delete (removing
    // a shipment line can only reduce progress, never complete a stage).
    public record PartShipmentRecordedEvent(int Buyer, string Order, int Type, string Style) : IDomainEvent;
}
